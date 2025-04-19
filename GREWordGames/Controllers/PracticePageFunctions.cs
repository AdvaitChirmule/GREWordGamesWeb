using Firebase.Database;
using GREWordGames.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Common;
using System.Diagnostics;

namespace GREWordGames.Controllers
{
    public class PracticePageFunctions
    {
        private readonly CommonFunctions _commonFunctions;
        private readonly FirebaseFunctions _firebaseFunctions;
        private string _token;
        private string _uid;
        private ISession _session;
        public PracticePageFunctions(string token, string uid, ISession session)
        {
            _token = token;
            _uid = uid;
            _session = session;
            _commonFunctions = new CommonFunctions();
            _firebaseFunctions = new FirebaseFunctions(_token, _uid);
        }
        public async Task<(List<string>, List<string>, List<string>, List<string>)> SegregateByUserDifficulty()
        {
            string wordListRaw = await _firebaseFunctions.GetUserWordList();
            string proficiencyListRaw = await _firebaseFunctions.GetUserProficiencyList();

            List<string> wordList = _commonFunctions.ConvertStringToList(wordListRaw);
            List<string> proficiencyList = _commonFunctions.ConvertStringToList(proficiencyListRaw);

            List<string> newWords = [];
            List<string> difficultWords = [];
            List<string> goodWords = [];
            List<string> excellentWords = [];

            for (int i = 0; i < proficiencyList.Count; i++)
            {
                if (_commonFunctions.isNewWord(proficiencyList[i]))
                {
                    newWords.Add(wordList[i]);
                }
                else
                {
                    float proficiencyPercentage = float.Parse(_commonFunctions.ProficiencyPercentage(proficiencyList[i]));
                    if (proficiencyPercentage > 75)
                    {
                        difficultWords.Add(wordList[i]);
                    }
                    else if (proficiencyPercentage > 50)
                    {
                        goodWords.Add(wordList[i]);
                    }
                    else
                    {
                        excellentWords.Add(wordList[i]);
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

                    HashSet<int> indexSet = [];
                    List<string> consolidatedList = [];
                    Random random = new();
                    
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
                    return [];
            }
        }

        public async Task UpdateWordProficiencies(AllWords newProficiency)
        {
            UserMetadata userMetadata = await _firebaseFunctions.GetUserDetails();

            List<string> wordList = _commonFunctions.ConvertStringToList(userMetadata.words);
            List<string> proficiencyList = _commonFunctions.ConvertStringToList(userMetadata.proficiency);

            Dictionary<string, int> originalToRandom = [];

            for (int i = 0; i < newProficiency.words.Count; i++)
            {
                originalToRandom[newProficiency.words[i]] = i;
            }

            for (int i = 0; i < wordList.Count; i++)
            {
                int newReference = originalToRandom[wordList[i]];
                if (newProficiency.outcome[newReference] == "1")
                {
                    proficiencyList[i] = _commonFunctions.increaseProficiencyByOne(proficiencyList[i], false);
                }
                else if (newProficiency.outcome[newReference] == "2")
                {
                    proficiencyList[i] = _commonFunctions.increaseProficiencyByOne(proficiencyList[i], true);
                }
            }

            string proficiencyListRaw = _commonFunctions.ConvertListToString(proficiencyList);

            userMetadata.proficiency = proficiencyListRaw;

            await _firebaseFunctions.SetUser(userMetadata);
        }
    }
}
