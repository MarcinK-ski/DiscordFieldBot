using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace DiscordFieldBot
{
    public class CalculateChanceBot
    {
        private static readonly string _FILE_NAME = Helper.config["ChancesBinary"];
        private static ulong _mainChannelId = ulong.Parse(Helper.channels["Chances"]);

        private static Dictionary<string, Subject> _subjects = new Dictionary<string, Subject>();
        private static ISocketMessageChannel _mainChannel = (ISocketMessageChannel)DiscordConnection.socketClient.GetChannel(_mainChannelId);
        
        public static void DeserializeDictionary()
        {
            Dictionary<string, Subject> sub = (Dictionary<string, Subject>)Helper.BinaryDeserializeObject(_FILE_NAME);

            if (sub != null)
            {
                _subjects = sub;
            }
        }

        public static async Task HandleComandAsync(string givenCommand, SocketMessage message)
        {
            if (message.Channel.Id == _mainChannelId)
            {
                CommandParameters commandParameters = new CommandParameters(givenCommand.Split(' '));

                int parametersCount = commandParameters.CountParameters;
                string command = commandParameters.Command;
                string resultToSend = null;
                string modificationCompletedStatement = null;

                if (parametersCount >= 0)
                {
                    if (new Regex("(przedmiot)|p$").IsMatch(command))
                    {
                        var trySubjectResult = await TrySubject(parametersCount, commandParameters, modificationCompletedStatement);

                        resultToSend = trySubjectResult.S1;
                        modificationCompletedStatement = trySubjectResult.S2;
                    }
                    else if (new Regex("(ocena)|o$").IsMatch(command))
                    {
                        resultToSend = TryMark(parametersCount, commandParameters, ref modificationCompletedStatement);
                    }
                    else if (new Regex("(szanse)|s$").IsMatch(command))
                    {
                        resultToSend = TryChance();
                    }
                    else if ("pomoc" == command)
                    {
                        resultToSend = TryHelp();
                    }


                    await _mainChannel.SendMessageAsync(resultToSend);

                    if (!string.IsNullOrWhiteSpace(modificationCompletedStatement))
                    {
                        try
                        {
                            Helper.BinarySerializeObject(_FILE_NAME, _subjects);
                            await _mainChannel.SendMessageAsync(modificationCompletedStatement);
                        }
                        catch(Exception ex)
                        {
                            await _mainChannel.SendMessageAsync("Wystąpł wyjątek, podczas tej operacji :(");
                            Helper.ConsoleWriteExceptionInfo(ex);
                        }
                    }
                }
            }
        }

        private static async Task<Strings> TrySubject(int parametersCount, CommandParameters commandParameters, string modificationCompletedStatement)
        {
            if (parametersCount < 1)
            {
                return new Strings { S1 = "Chcesz `dodać` czy `wyświetlić` przedmiot? Wpisz odpowiednią komendę..." };
            }

            if (new Regex("(wy[s|ś]wietl)|w").IsMatch(commandParameters.Parameters[0].Trim()))
            {
                string resultToSend = "Oto lista dodanych przedmiotow:\n";
                foreach (KeyValuePair<string, Subject> item in _subjects)
                {
                    resultToSend += $"*  `{item.Key}` z wagą: {item.Value.SubjectWeight}\n";
                }

                return new Strings { S1 = resultToSend };
            }
            else if (new Regex("(dodaj)|d").IsMatch(commandParameters.Parameters[0].Trim()))
            {
                if (parametersCount < 3)
                {
                    return new Strings { S1 = "Brakuje jednego z argumentów. Pamietaj o `nazwie` i `wadze` przedmiotu!" };
                }

                if (!sbyte.TryParse(commandParameters.Parameters[2].Trim(), out sbyte weight))
                {
                    return new Strings { S1 = "Problem z castowaniem `wagi przedmiotu`" };
                }

                if (await AddNewSubject(commandParameters.Parameters[1].Trim(), weight))
                {
                    modificationCompletedStatement = "Przedmiot dodano pomyślnie!";

                    return new Strings { S1 = "Dodawanie przedmiotu...", S2 = modificationCompletedStatement };
                }
            }

            return new Strings { S1 = "Nieznany błąd *TrySubject()*" };
        }
        
        private static string TryMark(int parametersCount, CommandParameters commandParameters, ref string modificationCompletedStatement)
        {
            modificationCompletedStatement = null;

            if (parametersCount < 1)
            {
                return "Chcesz `dodać` czy `wyświetlić` oceny? Wpisz odpowiednią komendę...";
            }

            string addToSubject = null;

            if (parametersCount > 1)
            {
                addToSubject = commandParameters.Parameters[1];
            }

            if (new Regex("(dodaj)|d$").IsMatch(commandParameters.Parameters[0].Trim()))
            {
                if (parametersCount < 4)
                {
                    return "Brakuje jednego z argumentów. Pamietaj o `nazwie` i `wadze` przedmiotu!";
                }

                if (!float.TryParse(commandParameters.Parameters[2], out float ocena))
                {
                    return "Problem z castowaniem `oceny`";
                }

                if (!sbyte.TryParse(commandParameters.Parameters[3], out sbyte waga))
                {
                    return "Problem z castowaniem `wagi oceny`";
                }

                if (_subjects.ContainsKey(addToSubject))
                {
                    _subjects[addToSubject].AddNewMark(ocena, waga);
                    modificationCompletedStatement = "Poprawnie dodano ocene!";

                    return "Dodawanie oceny...";
                }
                else
                {
                    return "Taki przedmiot nie istnieje";
                }
            }
            else if (new Regex("(wy[s|ś]wietl)|w$").IsMatch(commandParameters.Parameters[0].Trim()))
            {

                if (addToSubject != null)
                {
                    if(!_subjects.ContainsKey(addToSubject))
                    {
                        return "Brak poprawnego przedmiotu do wyświetlenia";
                    }
                }

                StringBuilder resultToSend = new StringBuilder();

                foreach (var subject in _subjects)
                {
                    Subject currentSubject = subject.Value;

                    if(addToSubject == null)
                    {
                        resultToSend.Append($"{currentSubject.ShowSubjectDetails(true)}");
                    }
                    else
                    {
                        if(subject.Key == addToSubject)
                        {
                            return currentSubject.ShowSubjectDetails();
                        }
                    }
                }

                if(resultToSend.Length > 2000)
                {
                    return "Nie można wyświetlić wszystkich ocen ze względu na ograniczenia ilości znaków";
                }
                else if(resultToSend.Length == 0)
                {
                    return "Brak ocen do wyświetlenia";
                }

                return resultToSend.ToString();
            }
            else if (new Regex("(archiwum)|a$").IsMatch(commandParameters.Parameters[0].Trim()))
            {
                if(parametersCount > 1)
                {
                    if (new Regex("(semestry)|s$").IsMatch(commandParameters.Parameters[1].Trim()))
                    {
                        StringBuilder toReturn = new StringBuilder();

                        foreach (var subject in _subjects)
                        {
                            toReturn.AppendLine($"Dla przedmiotu: {subject.Key} {subject.Value.GetAllTerms()}");
                        }

                        return toReturn.ToString();
                    }

                    if (parametersCount > 3)
                    {
                        if (!Int32.TryParse(commandParameters.Parameters[2], out int yearNo))
                        {
                            return "Problem z castowaniem `numeru roku`";
                        }

                        if (!Int32.TryParse(commandParameters.Parameters[3], out int termNo))
                        {
                            return "Problem z castowaniem `numeru semestru`";
                        }


                        if (new Regex("(wyswietl)|w$").IsMatch(commandParameters.Parameters[1].Trim()))
                        {
                            StringBuilder toReturn = new StringBuilder();

                            foreach (var subject in _subjects)
                            {
                                toReturn.Append(_subjects[subject.Key].GetArchivizedMarks(new Term(yearNo, termNo)));
                            }

                            return toReturn.ToString();
                        }
                        else if (new Regex("(archiwizuj)|a$").IsMatch(commandParameters.Parameters[1].Trim()))
                        {
                            bool isSomethingArchived = false;

                            foreach (var subject in _subjects)
                            {
                                if(_subjects[subject.Key].IsTermArchived(new Term(yearNo, termNo)))
                                {
                                    modificationCompletedStatement += $"Przedmiot {subject.Key} już został zarchiwizowany dla takiego semestru! \n";
                                    continue;
                                }

                                _subjects[subject.Key].ArchivizeMarks(yearNo, termNo);
                                isSomethingArchived = true;
                            }

                            if(isSomethingArchived)
                            {
                                modificationCompletedStatement += "\n\nZarchiwizowano!";
                            }
                            else
                            {
                                modificationCompletedStatement += "\n\nNie było czego archiwizować!";
                            }

                            return "Archiwizacja ocen...\n\n";
                        }
                    }
                    else
                    {
                        return "Podano za mało parametrów! Template: *a w/a <numer_roku> <numer_semestru>";
                    }
                }
                else
                {
                    return "Nie podano pocji dla *archiwum*";
                }
            }

            return "Nieznany błąd *TryMark()*";
        }

        private static string TryChance()
        {
            float avg = 0;
            int i = 0;
            foreach (var item in _subjects)
            {
                if (item.Value.GetLastMarksAvg() > 0)
                {
                    avg += ((item.Value.GetLastMarksAvg(true) * 100) / item.Value.Get100ProcentOfMarks()) * item.Value.SubjectWeight;   //Z proporcji * waga przedmiotu

                    i += item.Value.SubjectWeight;
                }
            }

            if (i < 1)
                return "Nie zostay wpisane jeszcze żadne oceny.";

            avg /= i;

            return $"Twoje szanse wynoszą: *{avg}*";
        }

        private static string TryHelp()
        {
            char botChar = Bots.GetBotTypeChar(Bots.BotType.ChanceCalculateBot);

            return $"Każda komenda musi zaczynać się znakiem: `{botChar}`" +
                    $"\nFormat wpisywania komendy: `{botChar}komenda parametr1 parametr2 parametr3 itd...` " +
                    $"\nLista komend: " +
                    $"\n    - `przedmiot` lub `p` - dodawanie nowego przedmiotu" +
                    $"\n            parametr1 => `dodaj` albo `d` lub `wyswietl` albo `w` " +
                    $"\n            *PONIZSZE parametry występują jedynie w przypadku wybrania operacji `dodaj`*" +
                    $"\n            parametr2 => `nazwa` *(BEZ SPACJI W NAZWIE!)*" +
                    $"\n            parametr3 => `wagaPrzedmiotu` *(1-10)*\n\n" +
                    $"\n    - `ocena` lub `o` - dodanie nowej oceny," +
                    $"\n            parametr1 => `dodaj` albo `d` lub `wyswietl` albo `w` " +
                    $"\n            parametr2 => `nazwa przedmiotu`, " +
                    $"\n            *PONIZSZE parametry występują jedynie w przypadku wybrania operacji `dodaj`*" +
                    $"\n            parametr3 => `ocena`, " +
                    $"\n            parametr4 => `wagaOceny` *(1-{Marks.MAX_WEIGHT})*\n" +
                    $"\n        + (opcja rozpoczynana od `o`, ponieważ należy do podmenu *ocen*) `archiwum` lub `a` - archiwizowanie ocen z semestru" +
                    $"\n            parametr1 => `semestry` albo `s` lub `wyswietl` albo `w` lub `archiwizuj` albo `a` " +
                    $"\n            *PONIZSZE parametry występują jedynie w przypadku wybrania operacji `archiwizuj` lub `wyswietl`*" +
                    $"\n            parametr2 => `Numer roku` (np. 1 lub 2 lub 3. !!!!NIE 2018!!!!" +
                    $"\n            parametr3 => `Numer semestru`\n\n" +
                    $"\n    - `obecnosc` lub `ob` - dodanie nowej nieobecnosci (nie zaimplemenotwano!),\n\n" +
                    $"\n    - `szanse` lub `s` - sprawdza jakie są szanse przy aktualnym stanie,\n\n" +
                    $"\n    - `pomoc` - wyświetla pomoc, czyli tę listę komend.\n\n";
        }

        private static async Task<bool> AddNewSubject(string subjectName, sbyte subjectWeight)
        {
            if (_subjects.ContainsKey(subjectName))
            {
                await _mainChannel.SendMessageAsync("Taki przedmiot juz istnieje");
                return false;
            }
            else
            {
                _subjects.Add(subjectName, new Subject(subjectName, subjectWeight));
                return true;
            }
        }
    }
}
