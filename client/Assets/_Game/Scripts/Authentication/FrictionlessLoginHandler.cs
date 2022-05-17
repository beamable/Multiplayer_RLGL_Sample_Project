namespace _Game.Features.Authentication
{
    public class FrictionlessLoginHandler : AuthenticationHandler
    {   
        public override void Login() { }

        protected override void Awake()
        {
            base.Awake();
            OnLoginSuccess?.Invoke();
        }
    }
}