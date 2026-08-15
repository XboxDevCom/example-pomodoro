using System;

namespace Pomodoro.Models
{
    /// <summary>
    /// Repräsentiert den Typ der aktuellen Pomodoro-Sitzung.
    /// </summary>
    public enum SessionType
    {
        /// <summary>
        /// Fokus-Phase (Arbeitsphase).
        /// </summary>
        Focus,

        /// <summary>
        /// Kurze Pause.
        /// </summary>
        ShortBreak,

        /// <summary>
        /// Lange Pause nach mehreren Fokus-Phasen.
        /// </summary>
        LongBreak
    }

    /// <summary>
    /// Verwaltet die Logik einer Pomodoro-Timer-Sitzung, einschließlich
    /// Fokus-Phasen, Pausen und Sitzungszählung.
    /// </summary>
    public sealed class PomodoroSession
    {
        /// <summary>
        /// Dauer einer Fokus-Phase in Minuten.
        /// </summary>
        public const int FocusDurationMinutes = 25;

        /// <summary>
        /// Dauer einer kurzen Pause in Minuten.
        /// </summary>
        public const int ShortBreakDurationMinutes = 5;

        /// <summary>
        /// Dauer einer langen Pause in Minuten.
        /// </summary>
        public const int LongBreakDurationMinutes = 15;

        /// <summary>
        /// Anzahl der Fokus-Phasen vor einer langen Pause.
        /// </summary>
        public const int SessionsBeforeLongBreak = 4;

        private static readonly TimeSpan FocusDuration = TimeSpan.FromMinutes(FocusDurationMinutes);
        private static readonly TimeSpan ShortBreakDuration = TimeSpan.FromMinutes(ShortBreakDurationMinutes);
        private static readonly TimeSpan LongBreakDuration = TimeSpan.FromMinutes(LongBreakDurationMinutes);

        /// <summary>
        /// Ruft den Typ der aktuellen Sitzung ab oder legt diesen fest.
        /// </summary>
        public SessionType CurrentType { get; private set; }

        /// <summary>
        /// Ruft die Gesamtdauer der aktuellen Sitzung ab.
        /// </summary>
        public TimeSpan Duration { get; private set; }

        /// <summary>
        /// Ruft die verbleibende Zeit der aktuellen Sitzung ab.
        /// </summary>
        public TimeSpan RemainingTime { get; private set; }

        /// <summary>
        /// Ruft einen Wert ab, der angibt, ob der Timer aktiv ist.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Ruft die Anzahl der abgeschlossenen Fokus-Phasen ab.
        /// </summary>
        public int CompletedFocusSessions { get; private set; }

        /// <summary>
        /// Ruft einen Wert ab, der angibt, ob als nächste Sitzung eine lange Pause folgt.
        /// </summary>
        public bool IsLongBreakNext => CompletedFocusSessions > 0 && CompletedFocusSessions % SessionsBeforeLongBreak == 0;

        /// <summary>
        /// Startet eine neue Fokus-Phase.
        /// </summary>
        public void StartFocus()
        {
            CurrentType = SessionType.Focus;
            Duration = FocusDuration;
            RemainingTime = Duration;
            IsActive = true;
        }

        /// <summary>
        /// Startet eine kurze Pause.
        /// </summary>
        public void StartShortBreak()
        {
            CurrentType = SessionType.ShortBreak;
            Duration = ShortBreakDuration;
            RemainingTime = Duration;
            IsActive = true;
        }

        /// <summary>
        /// Startet eine lange Pause. Diese sollte nur nach der konfigurierten
        /// Anzahl an Fokus-Phasen aufgerufen werden.
        /// </summary>
        public void StartLongBreak()
        {
            CurrentType = SessionType.LongBreak;
            Duration = LongBreakDuration;
            RemainingTime = Duration;
            IsActive = true;
        }

        /// <summary>
        /// Verringert die verbleibende Zeit um eine Sekunde.
        /// </summary>
        /// <returns>True, wenn die Sitzung abgeschlossen ist; andernfalls False.</returns>
        public bool Tick()
        {
            RemainingTime = RemainingTime.Subtract(TimeSpan.FromSeconds(1));
            if (RemainingTime <= TimeSpan.Zero)
            {
                RemainingTime = TimeSpan.Zero;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Schließt die aktuelle Sitzung ab und aktualisiert die Zählung
        /// sowie die nächste Sitzung.
        /// </summary>
        public void CompleteSession()
        {
            IsActive = false;
            if (CurrentType == SessionType.Focus)
            {
                CompletedFocusSessions++;
            }
        }

        /// <summary>
        /// Setzt die gesamte Sitzung in den Anfangszustand zurück.
        /// </summary>
        public void Reset()
        {
            CurrentType = SessionType.Focus;
            Duration = FocusDuration;
            RemainingTime = Duration;
            IsActive = false;
            CompletedFocusSessions = 0;
        }

        /// <summary>
        /// Bestimmt den Typ der nächsten Sitzung basierend auf den
        /// abgeschlossenen Fokus-Phasen.
        /// </summary>
        /// <returns>Den Typ der nächsten Sitzung.</returns>
        public SessionType GetNextSessionType()
        {
            if (CurrentType == SessionType.Focus)
            {
                if (IsLongBreakNext)
                {
                    return SessionType.LongBreak;
                }
                return SessionType.ShortBreak;
            }
            return SessionType.Focus;
        }

        /// <summary>
        /// Formatiert eine Zeitspanne als "MM:SS"-Zeichenfolge.
        /// </summary>
        /// <param name="time">Die zu formatierende Zeitspanne.</param>
        /// <returns>Die formatierte Zeitzeichenfolge.</returns>
        public static string FormatTime(TimeSpan time)
        {
            return $"{(int)time.TotalMinutes:D2}:{time.Seconds:D2}";
        }
    }
}
