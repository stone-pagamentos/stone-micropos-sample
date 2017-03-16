using SimpleConsoleApp.PaymentCore;

namespace SimpleConsoleApp
{
    internal sealed class Program
    {
        public static void Main (params string[] args)
        {
            AuthorizationManager authManager = new AuthorizationManager();
            authManager.Run();
        }
    }
}
