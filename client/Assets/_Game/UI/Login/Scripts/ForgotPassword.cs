using Beamable;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class ForgotPassword : MonoBehaviour
{
    private IBeamableAPI _beamableAPI;
    private string _code;

    [SerializeField] private EmailLoginHandler loginHandler;

    public UnityEvent OnCodeSent;
    public UnityEvent OnFailedToSend;

    public UnityEvent OnChangePassword;
    public UnityEvent OnFailedToChangePassword;

    private async void OnEnable()
    {
        _beamableAPI = await API.Instance;
    }

    [UsedImplicitly]
    public void SendCode()
    {
        _beamableAPI.AuthService.IssuePasswordUpdate(loginHandler.email)
            .Then(_ =>
            {
                OnCodeSent?.Invoke();
            })
            .Error(error =>
            {
                Debug.LogError(error);
                OnFailedToSend?.Invoke();
            });
    }

    [UsedImplicitly]
    public void ConfirmChangePassword()
    {
        _beamableAPI.AuthService.ConfirmPasswordUpdate(_code, loginHandler.password)
            .Then(_ =>
            {
                OnChangePassword?.Invoke();
            })
            .Error(error =>
            {
                Debug.LogError(error);
                OnFailedToChangePassword?.Invoke();
            });
    }

    [UsedImplicitly]
    public void SetCode(string code)
    {
        _code = code;
    }
}
