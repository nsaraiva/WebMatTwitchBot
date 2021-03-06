﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebMatBot
{
    public class Sounds
    {
        public static bool TrollisActive { get; set; } = true;

        public static void RandomSound()
        {
            if (!CheckStatus())
                return;

            Random rdm = new Random();
            var files = GetFiles();

            var index = rdm.Next(files.Length);

            SpeakerCore.ExecuteMP3File(files[index]);
            //Task.Delay(500);
        }

        public static bool CheckStatus()
        {
            return TrollisActive;
        }

        public static string[] GetFiles()
        {
            var targetDirectory = @Directory.GetCurrentDirectory() + @"\Sounds";

            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            return fileEntries;
        }


        public static void Piada()
        {
            var files = GetFiles();
            var piada = files.Where(q=>q.Contains("rimshot.mp3")).FirstOrDefault();
            if (piada != null) SpeakerCore.ExecuteMP3File(piada);
        }

        public static void Clap()
        {
            var files = GetFiles();
            var piada = files.Where(q => q.Contains("aplausos.mp3")).FirstOrDefault();
            if (piada != null) SpeakerCore.ExecuteMP3File(piada);
        }

    }
}
