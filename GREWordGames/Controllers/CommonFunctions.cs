using GREWordGames.Models;
using System.Diagnostics;
namespace GREWordGames.Controllers
{
    public class CommonFunctions
    {
        public UserClass ConvertRawDataToList(UserMetadata userDetails)
        {
            var wordList = userDetails.words[1..^1].Split(", ").ToList();
            var dateAddedList = userDetails.dateAdded[1..^1].Split(", ").ToList();
            var proficiencyList = userDetails.proficiency[1..^1].Split(", ").ToList();

            var user = new UserClass { wordList = wordList, dateAddedList = dateAddedList, proficiencyList = proficiencyList};
            return user;
        }

        public List<string> ConvertStringToList(string str)
        {
            List<string> strList = str[1..^1].Split(", ").ToList();
            return strList;
        }

        public string ConvertListToString(List<string> strList)
        {
            string str = "[";
            foreach (var item in strList)
            {
                str = str + item + ", ";
            }
            str = str[0..^2] + "]";
            return str;
        }

        public bool CheckIfWordInDatabase(string wordList, string word)
        {
            string wordArrayStr = "[" + string.Join(",", wordList.Trim('[', ']').Split(',').Select(w => $"\"{w.Trim()}\"")) + "]";

            if (wordArrayStr.Contains("\"" + word + "\""))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public UserClass DeleteRow(UserClass user, int id)
        {
            user.wordList.RemoveAt(id);
            user.proficiencyList.RemoveAt(id);
            user.dateAddedList.RemoveAt(id);

            return user;
        }

        public bool isNewWord(string proficiency)
        {
            var proficiencyRaw = proficiency[1..^1];
            var divide = proficiencyRaw.Split('|');
            var denominator = int.Parse(divide[1]);
            if (denominator <= 5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public List<string> ProficiencyPercentage(List<string> proficiencies)
        {
            for (int i = 0; i < proficiencies.Count; i++)
            {
                var proficiencyRaw = proficiencies[i][1..^1];
                var divide = proficiencyRaw.Split('|');
                var numerator = int.Parse(divide[0]);
                var denominator = int.Parse(divide[1]);
                float percentage = 0;
                if (numerator != 0)
                {
                    percentage = (float)numerator / denominator;
                }
                proficiencies[i] = percentage.ToString("0.00");
            }
            return proficiencies;
        }

        public string ProficiencyPercentage(string proficiency)
        {
            var proficiencyRaw = proficiency[1..^1];
            var divide = proficiencyRaw.Split('|');
            var numerator = int.Parse(divide[0]);
            var denominator = int.Parse(divide[1]);
            float percentage = 0;
            if (numerator != 0)
            {
                percentage = (float)numerator / denominator;
            }
            proficiency = percentage.ToString("0.00");
            return proficiency;
        }

        public string increaseProficiencyByOne(string proficiency, bool success)
        {
            var proficiencyRaw = proficiency[1..^1];
            var divide = proficiencyRaw.Split("|");
            var numerator = int.Parse(divide[0]);
            var denominator = int.Parse(divide[1]);
            if (success)
            {
                numerator++;
            }
            denominator++;
            string newProficiency = '(' + numerator.ToString() + "|" + denominator.ToString() + ")";
            return newProficiency;

        }

        public List<string> ShuffleAndReturnKWords(List<string> words, int k)
        {
            k = Math.Min(k, words.Count);

            HashSet<int> indexSeen = new HashSet<int>();
            Random random = new Random();

            List<string> result = new List<string>();

            while (indexSeen.Count < k)
            {
                int newIndex = random.Next(k);
                if (indexSeen.Contains(newIndex))
                {
                    continue;
                }
                else
                {
                    indexSeen.Add(newIndex);
                    result.Add(words[newIndex]);
                }
            }

            return result;
        }

        public List<string> FindCommonWordsForNRounds(List<string> wordListHost, List<string> wordListGuest, int rounds)
        {
            HashSet<string> wordSetHost = new HashSet<string>();
            for (int i = 0;  i < wordListHost.Count; i++)
            {
                wordSetHost.Add(wordListHost[i]);
            }

            List<string> commonWords = new List<string>();
            for (int i = 0; i < wordListGuest.Count; i++)
            {
                if (wordSetHost.Contains(wordListGuest[i]))
                {
                    commonWords.Add(wordListGuest[i]);
                }
            }

            List<string> commonWordsShuffled = ShuffleAndReturnKWords(commonWords, rounds * 2);
            return commonWordsShuffled;
        }
    }
}
