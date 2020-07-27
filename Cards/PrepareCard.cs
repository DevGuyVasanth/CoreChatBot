using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Cards
{
    public static class PrepareCard
    {
        public static string ReadCard(string fileName)
        {
            string[] BuildPath = { ".", "Card", fileName };
            var filePath = Path.Combine(BuildPath);
            var fileRead = File.ReadAllText(Environment.CurrentDirectory + filePath);
            return fileRead;
        }

    }
}
