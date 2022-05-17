using System.Collections.Generic;
using UnityEngine;
using Fusion;
using BeamableExample.RedlightGreenLight.Character;
using System.Linq;

namespace BeamableExample.RedlightGreenLight
{
    public class PlayerManager : MonoBehaviour
    {
        private static List<PlayerCharacter> _allPlayers = new List<PlayerCharacter>();
        public static List<PlayerCharacter> allPlayers => _allPlayers;

        private static Queue<PlayerCharacter> _playerQueue = new Queue<PlayerCharacter>();

        public static System.Action<PlayerCharacter> PlayerAddedNotification;
        public static System.Action<PlayerCharacter> PlayerRemovedNotification;

        public static void HandleNewPlayers()
        {
            if (_playerQueue.Count > 0)
            {
                PlayerCharacter player = _playerQueue.Dequeue();

                player.Respawn(0);
            }
        }

        /// <summary>
        /// Return a list of player idexes that are alive.
        /// </summary>
        /// <returns></returns>
        public static List<int> PlayersAliveIndexes()
        {
            List<int> playersAlive = new List<int>();
            for (int i = 0; i < _allPlayers.Count; i++)
            {
                if (_allPlayers[i].IsActivated)
                    playersAlive.Add(i);
            }

            return playersAlive;
        }

        /// <summary>
        /// Return the ammount of alive players.
        /// </summary>
        /// <returns></returns>
        public static int PlayersActiveCount()
        {
            return _allPlayers.Count(p => p.IsActivated == true);
        }

        public static int PlayersAliveCount()
        {
            return _allPlayers.Count(p => p.State == PlayerCharacter.PlayerState.Active);
        }

        public static int PlayersDeadCount()
        {
            return _allPlayers.Count(p => p.State == PlayerCharacter.PlayerState.Dead);
        }

        public static int PlayersSafeCount()
        {
            return _allPlayers.Count(p => p.State == PlayerCharacter.PlayerState.Safe);
        }

        public static List<PlayerCharacter> GetActivePlayerCharacters()
        {
            List<PlayerCharacter> players = new List<PlayerCharacter>();
            for (int i = 0; i < _allPlayers.Count; i++)
            {
                if (_allPlayers[i].State == PlayerCharacter.PlayerState.Active)
                    players.Add(_allPlayers[i]);
            }

            return players;
        }

        public static List<PlayerCharacter> GetSafePlayerCharacters()
        {
            List<PlayerCharacter> players = new List<PlayerCharacter>();
            for (int i = 0; i < _allPlayers.Count; i++)
            {
                if (_allPlayers[i].State == PlayerCharacter.PlayerState.Safe)
                    players.Add(_allPlayers[i]);
            }

            return players;
        }

        public static List<PlayerCharacter> GetDeadPlayerCharacters()
        {
            List<PlayerCharacter> players = new List<PlayerCharacter>();
            for (int i = 0; i < _allPlayers.Count; i++)
            {
                if (_allPlayers[i].State == PlayerCharacter.PlayerState.Dead)
                    players.Add(_allPlayers[i]);
            }

            return players;
        }

        public static PlayerCharacter GetPlayerWithInputAuthority()
        {
            for (int i = 0; i < _allPlayers.Count; i++)
            {
                if (_allPlayers[i].Object.HasInputAuthority)
                    return _allPlayers[i];
            }

            return null;
        }

        public static PlayerCharacter GetFirstAlivePlayer()
        {
            for (int i = 0; i < _allPlayers.Count; i++)
            {
                if (_allPlayers[i].IsActivated)
                    return _allPlayers[i];
            }

            return null;
        }

        public static int GetFirstAlivePlayerIndex()
        {
            for (int i = 0; i < _allPlayers.Count; i++)
            {
                if (_allPlayers[i].IsActivated)
                    return i;
            }

            return -1;
        }

        public static int GetNextAlivePlayerByIndex(int index, int direction)
        {
            for (int i = 0; i < _allPlayers.Count; i++)
            {
                if (direction > 0)
                {
                    index += 1;

                    if (index >= _allPlayers.Count)
                        index = 0;
                }

                if (direction < 0)
                {
                    index -= 1;

                    if (index < 0)
                        index = _allPlayers.Count - 1;
                }

                if (_allPlayers[index].IsActivated)
                    return index;
            }
            
            return -1;
        }

        public static void AddPlayer(PlayerCharacter player)
        {
            Debug.Log("PlayerCharacter Added");

            int insertIndex = _allPlayers.Count;
            // Sort the player list when adding players
            for (int i = 0; i < _allPlayers.Count; i++)
            {
                if (_allPlayers[i].PlayerRefID > player.PlayerRefID)
                {
                    insertIndex = i;
                    break;
                }
            }

            _allPlayers.Insert(insertIndex, player);
            _playerQueue.Enqueue(player);

            PlayerHUDInfoUI.Instance.SetNumberPlayersText(_allPlayers.Count);

            PlayerAddedNotification?.Invoke(player);

            // TODO:: Get this to work with proxies.
            //CullingManager.AddPlayerToCulling(player);

        }

        public static void RemovePlayer(PlayerCharacter player)
        {
            if (player == null || !_allPlayers.Contains(player))
                return;

            Debug.LogFormat("PlayerManager - RemovePlayer({0})", player.PlayerRefID);

            _allPlayers.Remove(player);

            PlayerHUDInfoUI.Instance.SetNumberPlayersText(_allPlayers.Count);

            PlayerRemovedNotification?.Invoke(player);

            // TODO:: Get this to work with proxies.
            //CullingManager.RemovePlayerFromCulling(player);

            var highlightColor = ColorUtility.ToHtmlStringRGB(FeedManager.Instance.highlightColor);
            var message = $"<color=#{highlightColor}>{player._playerName}</color> has left the game.";
            FeedManager.Instance.AddFeed(message);
        }

        public static void ResetPlayerManager()
        {
            Debug.Log("Clearing Player Manager");

            // TODO:: We need to notify the active Game That the player manager has been reset.

            // Clear the player list.
            allPlayers.Clear();
        }

        public static PlayerCharacter GetPlayerFromID(int id)
        {
            if (id >= _allPlayers.Count || id < 0)
                return null;

            return _allPlayers[id];
        }

        public static PlayerCharacter Get(PlayerRef playerRef)
        {
            for (int i = _allPlayers.Count - 1; i >= 0; i--)
            {
                if (_allPlayers[i] == null || _allPlayers[i].Object == null)
                {
                    _allPlayers.RemoveAt(i);
                    Debug.Log("Removing null player");
                }
                else if (_allPlayers[i].Object.InputAuthority == playerRef)
                    return _allPlayers[i];
            }

            return null;
        }
    }
}