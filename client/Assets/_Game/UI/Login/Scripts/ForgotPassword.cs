using Beamable;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class ForgotPassword : MonoBehaviour
{
    private BeamContext _context;
    private string _code;

    [SerializeField] private EmailLoginHandler loginHandler;

    public UnityEvent OnCodeSent;
    public UnityEvent OnFailedToSend;

    public UnityEvent OnChangePassword;
    public UnityEvent OnFailedToChangePassword;

    private async void OnEnable()
    {
        _context = BeamContext.Default;
        await _context.OnReady;
    }

    [UsedImplicitly]
    public void SendCode()
    {
        _context.Api.AuthService.IssuePasswordUpdate(loginHandler.email)
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
        _context.Api.AuthService.ConfirmPasswordUpdate(_code, loginHandler.password)
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
