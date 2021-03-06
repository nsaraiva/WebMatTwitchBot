﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WebMatBot.Translate;

namespace WebMatBot
{
    public class SpeakerCore
    {
        //public static bool Speaker { get; set; } = false;
        public static Status Speaker { get; set; } = Status.Disabled;

        public static int TimeSleeping { get; set; } = 0; // em segundos

        private static IList<Func<Task>> Queue = new List<Func<Task>>();

        public static async Task Speak(string textToSpeech, string user,bool wait = true)
        {
            if (!await CheckStatus()) return;

            Sounds.RandomSound();

            SpeakerCore.PreSpeech(user);

            textToSpeech = textToSpeech.Replace("\"", "\"\"");

            // Command to execute PS  
            ExecutePowerShell($@"Add-Type -AssemblyName System.speech;  
            $speak = New-Object System.Speech.Synthesis.SpeechSynthesizer;
            $speak.Rate = -3;
            $speak.Speak(""{textToSpeech}"");"); // Embedd text  

        }

        public static void ExecutePowerShell(string command)
        {
            // create a temp file with .ps1 extension  
            var cFile = System.IO.Path.GetTempPath() + Guid.NewGuid() + ".ps1";

            //Write the .ps1  
            using (var tw = new System.IO.StreamWriter(cFile, false, Encoding.UTF8))
                tw.Write(command);
            

            // Setup the PS  
            var start =
                new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = "C:\\windows\\system32\\windowspowershell\\v1.0\\powershell.exe",
                    LoadUserProfile = false,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = $"-executionpolicy bypass -File {cFile}",
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
                };
            var p = System.Diagnostics.Process.Start(start);
            p.StartInfo.RedirectStandardOutput = true;
            var copyProcess = Process.GetCurrentProcess();
            p.WaitForExit();
        }

        public static void ExecuteMP3File(string path)
        {
            ExecutePowerShell($@"Add-Type -AssemblyName PresentationCore;
            $mediaPlayer = New-Object System.Windows.Media.MediaPlayer;
            do{{
            $mediaPlayer.Open(""{path}"");
            $musicaDuracao = $mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
            }}
            until($musicaDuracao)
            $mediaPlayer.Play();
            Start-Sleep -Milliseconds $musicaDuracao;");
        }

        public static void PreSpeech(string user)
        {
            string text = user + " diz: ";

            // Command to execute PS  
            ExecutePowerShell($@"Add-Type -AssemblyName System.speech;  
            $speak = New-Object System.Speech.Synthesis.SpeechSynthesizer;
            $speak.Speak(""{text}"");"); // Embedd text  
        }

        public static async Task QueueAdd (Func<Task> action)
        {
            if (await CheckStatus())
                lock (Queue)
                    Queue.Add(action);
        }

        public static async void Start()
        {
            do
            {
                try
                {
                    if (Speaker == Status.Disabled || Speaker == Status.Paused)
                        await Task.Delay(2000);
                    else
                    {
                        Func<Task> scoped = null;

                        //get from list
                        lock (Queue)
                        {
                            if (Queue.Count > 0)
                            {
                                scoped = Queue[0];
                            }
                        }

                        //execute and wait
                        if (scoped != null)
                        {
                            await scoped();

                            //update list
                            lock (Queue)
                                Queue.Remove(Queue[0]);
                        }

                        await Task.Delay(TimeSleeping*1000);
                    }
                }catch(Exception excpt)
                {
                    Console.WriteLine(excpt.Message) ;
                }
            } while (true) ;
        }

        public static async Task<bool> CheckStatus()
        {
            if (Speaker == Status.Disabled)
            {
                await Engine.Respond("O Speaker está off... peça o streamer para aciona-lo...");
                return false;
            }
            else
                return true;
        }
    }

    public enum Status
    {
        Disabled,
        Enabled,
        Paused,
    }
}
