using System;
using System.Xml;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Pomodoro.Models;

namespace Pomodoro
{
    /// <summary>
    /// Die Hauptseite der Pomodoro-Timer-Anwendung.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly PomodoroSession _session;
        private readonly DispatcherTimer _timer;

        /// <summary>
        /// Initialisiert eine neue Instanz der MainPage-Klasse und richtet
        /// Timer und Anzeige ein.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            _session = new PomodoroSession();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;

            UpdateDisplay();
        }

        /// <summary>
        /// Behandelt das Klick-Ereignis des Start-/Pause-Buttons.
        /// Startet oder pausiert den Timer entsprechend.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_session.IsActive)
            {
                _timer.Stop();
                _session.IsActive = false;
                StartButton.Content = "Weiter";
            }
            else
            {
                if (_session.RemainingTime <= TimeSpan.Zero)
                {
                    _session.StartFocus();
                }
                _session.IsActive = true;
                _timer.Start();
                StartButton.Content = "Pause";
            }
            UpdateDisplay();
        }

        /// <summary>
        /// Behandelt das Klick-Ereignis des Zurücksetzen-Buttons.
        /// Setzt die Sitzung in den Anfangszustand zurück.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _session.Reset();
            UpdateDisplay();
            StartButton.Content = "Start";
        }

        /// <summary>
        /// Behandelt das Klick-Ereignis des Überspringen-Buttons.
        /// Bricht die aktuelle Sitzung ab und wechselt zur nächsten Phase.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _session.CompleteSession();
            StartNextSession();
            UpdateDisplay();
        }

        /// <summary>
        /// Wird jede Sekunde vom DispatcherTimer aufgerufen.
        /// Aktualisiert die verbleibende Zeit und behandelt Sitzungsenden.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void Timer_Tick(object sender, object e)
        {
            bool sessionComplete = _session.Tick();

            if (sessionComplete)
            {
                _timer.Stop();
                _session.CompleteSession();

                string message;
                if (_session.CurrentType == SessionType.Focus)
                {
                    message = "Fokus-Phase abgeschlossen! Zeit für eine Pause.";
                }
                else
                {
                    message = "Pause beendet! Bereit für die nächste Fokus-Phase?";
                }
                ShowToastNotification(message);

                StartNextSession();
            }

            UpdateDisplay();
        }

        /// <summary>
        /// Startet die nächste Sitzung basierend auf dem aktuellen Sitzungstyp.
        /// </summary>
        private void StartNextSession()
        {
            SessionType nextType = _session.GetNextSessionType();
            switch (nextType)
            {
                case SessionType.Focus:
                    _session.StartFocus();
                    break;
                case SessionType.ShortBreak:
                    _session.StartShortBreak();
                    break;
                case SessionType.LongBreak:
                    _session.StartLongBreak();
                    break;
            }
            StartButton.Content = "Start";
        }

        /// <summary>
        /// Aktualisiert alle Anzeigeelemente auf der Seite.
        /// </summary>
        private void UpdateDisplay()
        {
            TimerDisplay.Text = PomodoroSession.FormatTime(_session.RemainingTime);

            string sessionTypeText;
            switch (_session.CurrentType)
            {
                case SessionType.Focus:
                    sessionTypeText = "Fokus-Phase";
                    break;
                case SessionType.ShortBreak:
                    sessionTypeText = "Kurze Pause";
                    break;
                case SessionType.LongBreak:
                    sessionTypeText = "Lange Pause";
                    break;
                default:
                    sessionTypeText = "Fokus-Phase";
                    break;
            }
            SessionTypeText.Text = sessionTypeText;

            SessionCountText.Text = $"Abgeschlossene Phasen: {_session.CompletedFocusSessions}";

            SessionType nextType = _session.GetNextSessionType();
            string nextSessionText;
            switch (nextType)
            {
                case SessionType.Focus:
                    nextSessionText = $"Nächste Phase: Fokus ({PomodoroSession.FocusDurationMinutes} Min)";
                    break;
                case SessionType.ShortBreak:
                    nextSessionText = $"Nächste Phase: Kurze Pause ({PomodoroSession.ShortBreakDurationMinutes} Min)";
                    break;
                case SessionType.LongBreak:
                    nextSessionText = $"Nächste Phase: Lange Pause ({PomodoroSession.LongBreakDurationMinutes} Min)";
                    break;
                default:
                    nextSessionText = $"Nächste Phase: Fokus ({PomodoroSession.FocusDurationMinutes} Min)";
                    break;
            }
            NextSessionText.Text = nextSessionText;

            if (_session.Duration > TimeSpan.Zero)
            {
                double totalSeconds = _session.Duration.TotalSeconds;
                double remainingSeconds = _session.RemainingTime.TotalSeconds;
                double progress = ((totalSeconds - remainingSeconds) / totalSeconds) * 100;
                ProgressIndicator.Value = progress;
            }
            else
            {
                ProgressIndicator.Value = 0;
            }

            if (_session.IsActive)
            {
                StartButton.Content = "Pause";
            }
            else if (_session.RemainingTime < _session.Duration && _session.RemainingTime > TimeSpan.Zero)
            {
                StartButton.Content = "Weiter";
            }
            else
            {
                StartButton.Content = "Start";
            }
        }

        /// <summary>
        /// Zeigt eine Toast-Benachrichtigung mit der angegebenen Nachricht an.
        /// </summary>
        /// <param name="message">Die anzuzeigende Nachricht.</param>
        private void ShowToastNotification(string message)
        {
            string toastXml = $@"
                <toast>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>Pomodoro-Phase abgeschlossen!</text>
                            <text>{message}</text>
                        </binding>
                    </visual>
                </toast>";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(toastXml);

            ToastNotification toast = new ToastNotification(xmlDoc);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
