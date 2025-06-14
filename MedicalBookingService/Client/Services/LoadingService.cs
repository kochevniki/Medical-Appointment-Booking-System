namespace MedicalBookingService.Client.Services
{
    public class LoadingService
    {
        public event Action? OnChange;
        private int _activeRequests = 0;

        public bool IsLoading => _activeRequests > 0;

        public void Show()
        {
            _activeRequests++;
            Console.WriteLine($"LoadingService: Show - Active Requests: {_activeRequests}");
            OnChange?.Invoke();
        }

        public void Hide()
        {
            _activeRequests = Math.Max(0, _activeRequests - 1);
            OnChange?.Invoke();
        }
    }
}
