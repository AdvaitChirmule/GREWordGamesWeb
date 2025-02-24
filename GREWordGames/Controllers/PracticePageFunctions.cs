using GREWordGames.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GREWordGames.Controllers
{
    public class PracticePageFunctions
    {
        private readonly CommonFunctions _commonFunctions;
        public PracticePageFunctions()
        {
            _commonFunctions = new CommonFunctions();
        }
        public (List<string>, List<string>, List<string>, List<string>) SegregateByUserDifficulty(UserClass user)
        {
            List<string> newWords = new List<string>();
            List<string> difficultWords = new List<string>();
            List<string> goodWords = new List<string>();
            List<string> excellentWords = new List<string>();

            for (int i = 0; i < user.proficiencyList.Count; i++)
            {
                if (_commonFunctions.isNewWord(user.proficiencyList[i]))
                {
                    newWords.Add(user.wordList[i]);
                }
                else
                {
                    float proficiencyPercentage = float.Parse(_commonFunctions.ProficiencyPercentage(user.proficiencyList[i]));
                    if (proficiencyPercentage > 75)
                    {
                        difficultWords.Add(user.wordList[i]);
                    }
                    else if (proficiencyPercentage > 50)
                    {
                        goodWords.Add(user.wordList[i]);
                    }
                    else
                    {
                        excellentWords.Add(user.wordList[i]);
                    }
                }
            }

            return (newWords, difficultWords, goodWords, excellentWords);
        }
    }
}
