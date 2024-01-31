﻿using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.Messages;
using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class EarnViewModel : BaseViewModel, IRecipient<QuizzesUpdatedMessage>
{
    private readonly IQuizService _quizService;

    private string quizDetailsPageUrl = "earn/details";

    public ObservableCollection<QuizDto> Quizzes { get; set; } = new ();
    
    public ObservableCollection<QuizDto> CarouselQuizzes { get; set; } = new ();

    public ICommand OpenQuizCommand { get; set; }

    [ObservableProperty]
    private bool _isCarouselVisible;

    public EarnViewModel(IQuizService quizService)
    {
        _quizService = quizService;

        OpenQuizCommand = new Command<int>(
            execute:
            async (id) => 
            {
                var quiz = Quizzes.FirstOrDefault(q => q.Id == id);
                await OpenQuiz(id, quiz.Icon);
            },
            canExecute:
            (id) =>
            {
                return Quizzes.FirstOrDefault(q => q.Id == id).Passed == false;
            });

        WeakReferenceMessenger.Default.Register(this);
    }

    public async Task Initialise()
    {
        Quizzes = new ObservableCollection<QuizDto>();
        OnPropertyChanged(nameof(Quizzes));
        CarouselQuizzes = new ObservableCollection<QuizDto>();
        OnPropertyChanged(nameof(CarouselQuizzes));
        
        IsBusy = true;

        var quizzes = await _quizService.GetQuizzes();
        
        foreach (var quiz in quizzes)
        {
            Quizzes.Add(quiz);
            
            if (quiz.IsCarousel)
            {
                IsCarouselVisible = true;
                CarouselQuizzes.Add(quiz);
            }
        }
        
        IsBusy = false;
    }

    private async Task OpenQuiz(int quizId, Icons icon)
    {
        await AppShell.Current.GoToAsync($"{quizDetailsPageUrl}?QuizId={quizId}&QuizIcon={icon}");
    }

    public async void Receive(QuizzesUpdatedMessage message)
    {
        await Initialise();
    }
}