using CommunityToolkit.Mvvm.ComponentModel;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class QuizResultPendingViewModel : ObservableObject
{
    private readonly Timer _timer;
    private int _numberOfCalls;

    private List<(int id, string message)> _loadingPhrases = new List<(int id, string message)>
    {
        (id: 1, message: "Summoning answers from our quiz master..."),
        (id: 2, message: "Extracting brilliance from our genius AI companion..."),
        (id: 3, message: "Harvesting insights from the all-knowing AI guru..."),
        (id: 4, message: "Retrieving wisdom from the depths of the AI genius..."),
        (id: 5, message: "Snatching enlightenment from our brainy AI overlord...")
    };

    private List<int> _discardedLoadingPhrases = [];

    [ObservableProperty]
    private string _loadingPhrase;
    
    public QuizResultPendingViewModel()
    {
        int periodInSeconds = 3;
        var firstPhrase = _loadingPhrases[new Random().Next(_loadingPhrases.Count)];
        LoadingPhrase = firstPhrase.message;
        _discardedLoadingPhrases.Add(firstPhrase.id);
        _timer = new Timer(TimerCallback, null, TimeSpan.FromSeconds(periodInSeconds), TimeSpan.FromSeconds(periodInSeconds));
    }

    public event Action TimeLapsed;

    public void KillTimer()
    {
        _timer.Dispose();
    }

    private void TimerCallback(object state)
    {
        if (_numberOfCalls < 20)
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                var randomPhrase = _loadingPhrases
                    .Where(phrase => !_discardedLoadingPhrases.Contains(phrase.id))
                    .OrderBy(_ => Guid.NewGuid())
                    .FirstOrDefault();

                LoadingPhrase = randomPhrase.message;
                _discardedLoadingPhrases.Add(randomPhrase.id);
                if (_discardedLoadingPhrases.Count == _loadingPhrases.Count)
                {
                    _discardedLoadingPhrases.Clear();
                }

                TimeLapsed?.Invoke();
            });
            _numberOfCalls++;
        }
        else
        {
            KillTimer();
        }
    }
}