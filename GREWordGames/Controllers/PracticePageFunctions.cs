using GREWordGames.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Common;

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

        public List<string> ReorderWordsBasedOnDifficulty((List<string>, List<string>, List<string>, List<string>) wordSegregated, int difficulty)
        {
            switch(difficulty)
            {
                case 0:
                    List<string> allWordsList =
                    [
                        .. wordSegregated.Item1,
                        .. wordSegregated.Item2,
                        .. wordSegregated.Item3,
                        .. wordSegregated.Item4,
                    ];

                    HashSet<int> indexSet = new HashSet<int>();
                    List<string> consolidatedList = new List<string>();
                    Random random = new Random();
                    
                    while (indexSet.Count < allWordsList.Count)
                    {
                        int newIndex = random.Next(allWordsList.Count);
                        if (!indexSet.Contains(newIndex))
                        {
                            indexSet.Add(newIndex);
                            consolidatedList.Add(allWordsList[newIndex]);
                        }
                    }    
                    return consolidatedList;


                default:
                    return new List<string>();
            }
        }
    }
}
