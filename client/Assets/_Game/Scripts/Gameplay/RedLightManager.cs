using System.Collections;
using UnityEngine;
using BeamableExample.Helpers;
using System;
using UnityEngine.Rendering.Universal;
using BeamableExample.RedlightGreenLight.Character;
using System.Collections.Generic;
using Fusion;
using UnityEngine.Rendering;
using Beamable;
using System.Threading.Tasks;
using Beamable.Common.Content;
using Beamable.Common.Shop;

namespace BeamableExample.RedlightGreenLight
{
    public class RedLightManager : NetworkBehaviour
    {
        public bool Run = true;
        public enum PLAYSTATE { GREEN, GREEN_ONE, GREEN_TWO, GREEN_THREE, RED, RED_ONE, RED_TWO, RED_THREE, IDLE }

        public float goTimeMin = 4.0f;
        public float goTimeMax = 4.0f;
        public float stopTimeMin = 4.0f;
        public float stopTimeMax = 4.0f;

        public MeshRenderer[] lightRenderers;
        public Color warningColor;
        public Color stopColor;
        public Color goColor;

        [Networked(OnChanged = nameof(OnStateChanged))]
        public PLAYSTATE playState { get; set; } = PLAYSTATE.IDLE;

        private Coroutine _redlightRoutine;

        public ForwardRendererData rendererData;
        public event EventHandler GreenLightEvent;
        public event EventHandler RedLightEvent;
        public event EventHandler<int> TransitionLightEvent;

        public Collider winningCollider;
        public GameObject startLineForceField;
        public float arenaRadius;

        public Transform aimLOC;
        public float _lineOfSightWidth = 0.3f;
        [SerializeField]
        [Tooltip("Mask used by RedLight Game Raycasting.")]
        private LayerMask _lineOfSightMask;
        [SerializeField]
        private float _lineOfSightTolerance = 1f;
        private float _lineOfSightTimer = 0f;
        private const float _failThreshold = 0.5f;

        [SerializeField] private MatchTimer _matchTimer;
        public float matchTime = 180f;

        /// <summary>
        /// This is the local(InputAuth) player. To be used on each client.
        /// </summary>
        private PlayerCharacter _inputAuthPlayer;

        [SerializeField] private int _killGreenPoints = 10;
        [SerializeField] private int _killRedPoints = 15;
        [SerializeField] private int _MaxFinishPoints = 20;
        [SerializeField] private int _MaxFailPoints = 5;

        [SerializeField] private int maxSafePlayers = 10;

        [Networked(OnChanged = nameof(OnSafePlayerCountChange))]
        public int _safePlayerCount { get; set; } = -1;

        private List<PlayerCharacter> _safePlayers = new List<PlayerCharacter>();

        [SerializeField] private SimGameTypeRef _redLightGameTypeRef;
        
        private SimGameType _redLightGameType;

        public void Awake()
        {
            winningCollider.enabled = false;
        }

        public override void Spawned()
        {
            PlayerManager.PlayerRemovedNotification += OnPlayerRemoved;

            InitializeRefs();
        }

        public async void InitializeRefs()
        {
            await ResolveRefs();
        }

        /// <summary>
        /// Resolves the storeRef and currencyRef to get their content.
        /// </summary>
        private async Task ResolveRefs()
        {
            _redLightGameType = await _redLightGameTypeRef.Resolve();

            GetFloatRule("MatchTime", out matchTime);

            if (_matchTimer == null)
                _matchTimer = FindObjectOfType<MatchTimer>(true);

            if (_matchTimer != null)
            {
                _matchTimer.SetTimerText(matchTime);
            }

            GetIntRule("MaxSafePlayers", out maxSafePlayers);
            GetIntRule("KillGreenPoints", out _killGreenPoints);
            GetIntRule("KillRedPoints", out _killRedPoints);
            GetIntRule("MaxFinishPoints", out _MaxFinishPoints);
            GetIntRule("MaxFailPoints", out _MaxFailPoints);

            Reset();
        }

        private void GetFloatRule(string ruleName, out float property)
        {
            var rule = _redLightGameType.stringRules.Find(p => p.property == ruleName);
            if (rule != null)
                float.TryParse(rule.value, out property);
            else
                property = 0.0f;
        }

        private void GetIntRule(string ruleName, out int property)
        {
            var rule = _redLightGameType.stringRules.Find(p => p.property == ruleName);
            if (rule != null)
                int.TryParse(rule.value, out property);
            else
                property = 0;
        }

        public void Reset()
        {
            if (Object.HasStateAuthority)
            {
                playState = PLAYSTATE.IDLE;
                _safePlayerCount = 0;
            }

            ResetLights();

            startLineForceField.SetActive(true);
            _safePlayers.Clear();

            // We have to force the onchanged method here because fusion will not call it on spawned objects initially.
            OnStateChanged();
            SafePlayerCountChanged();
            _matchTimer.ResetTimer(matchTime);
        }

        public override void Render()
        {
            if (_inputAuthPlayer != null)
            {
                LineOfSightTest();
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority)
            {
                if (playState != PLAYSTATE.IDLE)
                {
                    // Check for game completion.
                    if (_matchTimer.TimeLeft <= 0.0f)
                    {
                        // The Time Expired.
                        StopGame();
                    }
                }
            }
        }

        private void LineOfSightTest()
        {
            // We only want to run on the Client/Player with InputAuth.
            if (_inputAuthPlayer == null || _inputAuthPlayer.State != PlayerCharacter.PlayerState.Active || _inputAuthPlayer.Object.HasInputAuthority == false)
                return;

            // Get the direction to the from the eye to the player.
            Vector3 toPlayerCenter = (_inputAuthPlayer.transform.position + Vector3.up * 1.6f) - aimLOC.transform.position;
            Vector3 toPlayerLeft = (_inputAuthPlayer.transform.position + Vector3.up * 1.3f + _inputAuthPlayer.transform.right * -_lineOfSightWidth) - aimLOC.transform.position;
            Vector3 toPlayerRight = (_inputAuthPlayer.transform.position + Vector3.up * 1.3f + _inputAuthPlayer.transform.right * _lineOfSightWidth) - aimLOC.transform.position;

            RaycastHit hitInfo;
            int hitCount = 0;

            Debug.DrawRay(aimLOC.transform.position, toPlayerCenter.normalized * 1000.0f, Color.red);
            Debug.DrawRay(aimLOC.transform.position, toPlayerLeft.normalized * 1000.0f, Color.red);
            Debug.DrawRay(aimLOC.transform.position, toPlayerRight.normalized * 1000.0f, Color.red);

            if (Physics.Raycast(aimLOC.transform.position, toPlayerCenter, out hitInfo, 1000.0f, _lineOfSightMask, QueryTriggerInteraction.Ignore))
            {
                if (hitInfo.collider.gameObject.layer == _inputAuthPlayer.KCC.Collider.gameObject.layer)
                {
                    hitCount++;
                }
            }
            if (Physics.Raycast(aimLOC.transform.position, toPlayerLeft, out hitInfo, 1000.0f, _lineOfSightMask, QueryTriggerInteraction.Ignore))
            {
                if (hitInfo.collider.gameObject.layer == _inputAuthPlayer.KCC.Collider.gameObject.layer)
                {
                    hitCount++;
                }
            }
            if (Physics.Raycast(aimLOC.transform.position, toPlayerRight, out hitInfo, 1000.0f, _lineOfSightMask, QueryTriggerInteraction.Ignore))
            {
                if (hitInfo.collider.gameObject.layer == _inputAuthPlayer.KCC.Collider.gameObject.layer)
                {
                    hitCount++;
                }
            }
            if (hitCount > 1)
            {
                hitCount++;
                PlayerHUDInfoUI.Instance.SetVisibleIconUI(true);
                _inputAuthPlayer.hasBeenSeen = true;
                MotionTest();
            }
            else
            {
                PlayerHUDInfoUI.Instance.SetVisibleIconUI(false);
                _lineOfSightTimer = 0f;
            }
        }


        private void MotionTest()
        {
            if (playState == PLAYSTATE.RED || playState == PLAYSTATE.RED_THREE || playState == PLAYSTATE.RED_TWO || playState == PLAYSTATE.RED_ONE)
            {
                _lineOfSightTimer += Time.fixedDeltaTime;


                if (_lineOfSightTimer > _lineOfSightTolerance)
                {
                    if (_inputAuthPlayer.KCC.RenderData.RealVelocity.magnitude >= _failThreshold)
                    {
                        Debug.LogFormat("Motion Found by Player_{0}.", _inputAuthPlayer.PlayerRefID);

                        var highlightColor = ColorUtility.ToHtmlStringRGB(FeedManager.Instance.highlightColor);
                        var reason = $"Killed by <color=#{highlightColor}>Laser</color>";

                        _inputAuthPlayer.RPC_SetDamageInfo(3, GameManager.KillSource.LASER, WeaponType.LASER, DamageModifiers.NONE, PlayerRef.None);

                        // Send out the word to kill the player. The player has InputAuth.
                        _inputAuthPlayer.RPC_KillPlayer(GameManager.DisplayState.ELIMINATION, GameManager.KillSource.LASER, reason);
                    }
                }
            }
        }


        // This only happens on the Host/Server.
        private void OnTriggerEnter(Collider other)
        {
            if (!Object.HasStateAuthority)
                return;

            PlayerCharacter player = other.GetComponentInParent<PlayerCharacter>();
            if (player != null)
            {
                if (player.State != PlayerCharacter.PlayerState.Safe && player.State != PlayerCharacter.PlayerState.Dead)
                {
                    Debug.LogFormat("<color=blue>{0} ENTERED THE SAFE ZONE!!!!</color>", player.PlayerRefID);

                    player.SetPlayerSafe();

                    if (_safePlayers.Contains(player) == false)
                    {
                        _safePlayers.Add(player);
                        _safePlayerCount = _safePlayers.Count;
                        
                        if (_safePlayerCount == 1)
                        {
                            player.MarkFirstPlace();
                        }
                        player.AddToMatchesWon();

                        player.gameScore += CalculatePointsForFinish();

                        GameCompletionCheck();
                    }
                }
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_SendMessage(string message, RpcInfo info = default)
        {
            Debug.LogFormat("<color=yellow>RPC_SendMessage({0}) - Source = {1}</color>", message, info.Source);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_GameCompletionChecker(RpcInfo info = default)
        {
            Debug.Log("<color=yellow>RPC_GameCompletionChecker()</color>");

            GameCompletionCheck();
        }

        // The "Game" should only happen on the Host/Server.
        private void GameCompletionCheck()
        {
            if (!Object.HasStateAuthority)
                return;

            int aliveCount = PlayerManager.PlayersAliveCount();
            int deadCount = PlayerManager.PlayersDeadCount();
            int safeCount = PlayerManager.PlayersSafeCount();

            Debug.LogFormat("<color=yellow>GameCompletionCheck() - Safe = {0}, Alive = {1}, Dead = {2}, Total = {3}</color>", safeCount, aliveCount, deadCount, PlayerManager.allPlayers.Count);

            // Game Completion Checks.
            if (PlayerManager.PlayersActiveCount() == _safePlayers.Count)
            {
                // All living players have crossed the finish-line.
                StopGame();

            }
            else if (safeCount >= maxSafePlayers)
            {
                // Enough players crossed the finish-line so that we can consider the game completed.
                StopGame();
            }
            else if (aliveCount == 0)
            {
                // All players are dead.
                StopGame();
            }
        }

        public void BeginGame()
        {
            SetLocalPlayer();

            // Remove forceField.
            startLineForceField.SetActive(false);
            winningCollider.enabled = true;

            ResetLights();

            // TODO:: REMOVE "Run" Variable and all uses. This was only for testing purposes.
            if (!Run)
                return;

            if (Object.HasStateAuthority)
            {
                _matchTimer.StartTimer(matchTime);
                _redlightRoutine = StartCoroutine(CountDownAnimation());
            }
        }

        public void StopGame()
        {
            ResetLights();

            // Everything below here happens on the Host/Server.
            if (!Object.HasStateAuthority)
                return;

            Debug.Log("<color=yellow>STOPGAME()</color>");

            playState = PLAYSTATE.IDLE;

            if (_redlightRoutine != null)
                StopCoroutine(_redlightRoutine);
            _redlightRoutine = null;

            _matchTimer.StopTimer();

            var highlightColor = ColorUtility.ToHtmlStringRGB(FeedManager.Instance.highlightColor);
            var winReason = $"<color=#{highlightColor}>You Won!</color>";

            // Show the game completion screen to the safe players.
            List<PlayerCharacter> safePlayers = PlayerManager.GetSafePlayerCharacters();
            foreach (PlayerCharacter player in safePlayers)
            {
                player.RPC_Celebrate(winReason);
            }

            // Kill All Active Players and show the game completion screen.
            var killReason = $"<color=#{highlightColor}>End of Game</color>";
            List<PlayerCharacter> activePlayers = PlayerManager.GetActivePlayerCharacters();
            foreach (PlayerCharacter player in activePlayers)
            {
                player.gameScore += CalculateFailPoints(player);

                _inputAuthPlayer.RPC_SetDamageInfo(3, GameManager.KillSource.ENDOFGAME, WeaponType.NONE, DamageModifiers.NONE, PlayerRef.None);

                player.RPC_KillPlayer(GameManager.DisplayState.COMPLETION, GameManager.KillSource.ENDOFGAME, killReason);
            }

            // Get all players who are dead/spectators.
            List<PlayerCharacter> deadPlayers = PlayerManager.GetDeadPlayerCharacters();
            foreach (PlayerCharacter player in deadPlayers)
            {
                player.RPC_DisplayMatchCompletionUI();
            }
        }

        private IEnumerator CountDownAnimation()
        {
            while (true)
            {
                playState = PLAYSTATE.GREEN;

                // Let the players move around for a random time range.
                yield return new WaitForSeconds(UnityEngine.Random.Range(goTimeMin, goTimeMax));

                // Update the warning lights.
                playState = PLAYSTATE.GREEN_ONE;

                yield return new WaitForSeconds(1.0f);

                // Update the warning lights.
                playState = PLAYSTATE.GREEN_TWO;

                yield return new WaitForSeconds(1.0f);

                // Update the warning lights.
                playState = PLAYSTATE.GREEN_THREE;

                yield return new WaitForSeconds(1.0f);

                playState = PLAYSTATE.RED;

                yield return new WaitForSeconds(UnityEngine.Random.Range(goTimeMin, goTimeMax));

                // Update the warning lights.
                playState = PLAYSTATE.RED_ONE;

                yield return new WaitForSeconds(1.0f);

                // Update the warning lights.
                playState = PLAYSTATE.RED_TWO;

                yield return new WaitForSeconds(1.0f);

                // Update the warning lights.
                playState = PLAYSTATE.RED_THREE;

                yield return new WaitForSeconds(1.0f);
            }
        }
        public static void OnSafePlayerCountChange(Changed<RedLightManager> changed)
        {
            Debug.Log("<color=red>OnSafePlayerCountChange()</color>");

            if (changed.Behaviour)
                changed.Behaviour.SafePlayerCountChanged();
        }
        private void SafePlayerCountChanged()
        {
            Debug.LogFormat("<color=red>OnSafePlayerCountChanged() - Safe = {0}, Max = {1}</color>", _safePlayerCount, maxSafePlayers);

            PlayerHUDInfoUI.Instance.SetWinnerCirclePlayersText(_safePlayerCount, maxSafePlayers);
        }


        public static void OnStateChanged(Changed<RedLightManager> changed)
        {
            if (changed.Behaviour)
                changed.Behaviour.OnStateChanged();
        }

        public void OnStateChanged()
        {
            switch (playState)
            {
                case PLAYSTATE.IDLE:
                    {
                        ResetLights();
                    }
                    break;
                case PLAYSTATE.GREEN:
                    {
                        // It's go time.
                        LevelSoundManager.Instance.PlayGoTone();
                        SetLightColor(goColor);
                        GreenLightEvent?.Invoke(this, EventArgs.Empty);
                    }
                    break;
                case PLAYSTATE.GREEN_ONE:
                    {
                        lightRenderers[0].material.SetColor("_EmissionColor", warningColor);
                        LevelSoundManager.Instance.PlayCountdownTone();
                        TransitionLightEvent?.Invoke(this, 0);
                    }
                    break;
                case PLAYSTATE.GREEN_TWO:
                    {
                        lightRenderers[1].material.SetColor("_EmissionColor", warningColor);
                        LevelSoundManager.Instance.PlayCountdownTone();
                        TransitionLightEvent?.Invoke(this, 1);
                    }
                    break;
                case PLAYSTATE.GREEN_THREE:
                    {
                        lightRenderers[2].material.SetColor("_EmissionColor", warningColor);
                        LevelSoundManager.Instance.PlayCountdownTone();
                        TransitionLightEvent?.Invoke(this, 2);
                    }
                    break;
                case PLAYSTATE.RED:
                    {
                        LevelSoundManager.Instance.PlayStopTone();
                        SetLightColor(stopColor);
                        RedLightEvent?.Invoke(this, EventArgs.Empty);
                    }
                    break;
                case PLAYSTATE.RED_ONE:
                    {
                        LevelSoundManager.Instance.PlayCountdownTone();
                        TransitionLightEvent?.Invoke(this, 0);
                    }
                    break;
                case PLAYSTATE.RED_TWO:
                    {
                        LevelSoundManager.Instance.PlayCountdownTone();
                        TransitionLightEvent?.Invoke(this, 1);
                    }
                    break;
                case PLAYSTATE.RED_THREE:
                    {
                        LevelSoundManager.Instance.PlayCountdownTone();
                        TransitionLightEvent?.Invoke(this, 2);
                    }
                    break;
            }
        }

        public void SetLocalPlayer()
        {
            _inputAuthPlayer = PlayerManager.GetPlayerWithInputAuthority();
            if (_inputAuthPlayer != null)
            {
                _inputAuthPlayer.KilledCallback += OnPlayerKilled;
            }
        }

        // Only Happens on the Host/Server.
        private void OnPlayerRemoved(PlayerCharacter player)
        {
            Debug.LogFormat("<color=red>RedLightManager - OnPlayerRemoved({0})</color>", player.PlayerRefID);

            GameCompletionCheck();
        }

        // Happens on the InputAuth Client.
        private void OnPlayerKilled(PlayerCharacter player)
        {
            Debug.LogFormat("<color=red>RedLightManager - OnPlayerKilled({0})</color>", player.PlayerRefID);

            RPC_SendMessage("Hello!");
            RPC_GameCompletionChecker();
        }

        private void OnDestroy()
        {
            PlayerManager.PlayerRemovedNotification -= OnPlayerRemoved;
            if (_inputAuthPlayer != null)
            {
                _inputAuthPlayer.KilledCallback -= OnPlayerKilled;
            }
        }

        public void ResetLights()
        {
            SetLightColor(goColor);
        }

        private void SetLightColor(Color color)
        {
            // It's go time.
            foreach (MeshRenderer meshRenderer in lightRenderers)
            {
                meshRenderer.material.SetColor("_EmissionColor", color);
            }
        }

        public float GetMatchTimeLeft()
        {
            return _matchTimer.TimeLeft;
        }

        public int CaluculatePointsForKill()
        {
            int pointsAlloted = _killGreenPoints;
            switch (playState)
            {
                case PLAYSTATE.GREEN:
                case PLAYSTATE.GREEN_ONE:
                case PLAYSTATE.GREEN_TWO:
                case PLAYSTATE.GREEN_THREE:
                    {
                        pointsAlloted = _killGreenPoints;
                    }
                    break;
                case PLAYSTATE.RED:
                case PLAYSTATE.RED_ONE:
                case PLAYSTATE.RED_TWO:
                case PLAYSTATE.RED_THREE:
                    {
                        pointsAlloted = _killRedPoints;
                    }
                    break;
            }

            return pointsAlloted;
        }

        private int CalculatePointsForFinish()
        {
            float timeLeft = GetMatchTimeLeft();
            float percentage = timeLeft / matchTime;
            return Mathf.RoundToInt(_MaxFinishPoints * percentage);
        }

        private int CalculateFailPoints(PlayerCharacter player)
        {
            Vector3 toPlayerCenter = (player.transform.position) - aimLOC.transform.position;
            Vector3 projectedVector = Vector3.ProjectOnPlane(toPlayerCenter, Vector3.up);

            float percentage = 1.0f - projectedVector.magnitude / arenaRadius;

            return Mathf.RoundToInt(_MaxFailPoints * percentage);
        }
    }
}
