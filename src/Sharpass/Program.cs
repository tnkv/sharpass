﻿using System.CommandLine;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Sharpass;

const string lowerLettersEnglish = "qwertyuiopasdfghjklzxcvbnm";
const string upperLettersEnglish = "QWERTYUIOPASDFGHJKLZXCVBNM";
const string lowerLettersRussian = "йцукенгшщзхъфывапролджэячсмитьбюё";
const string upperLettersRussian = "ЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮЁ";
const string digitsDictionary = "1234567890";
const string specialCharsDictionary = """!@#$%^&*()_+-=`~/|\.,<>""";

var rootCommand = GetRootCommand();
await rootCommand.InvokeAsync(args);

return;

static Command GetRootCommand()
{
    var rootCommand = new RootCommand("Generates cryptographically strong password");

    var useRussianLettersOption = new Option<bool>(
        aliases: ["--russian", "-r"],
        description: "Use russian letters instead of english ones",
        getDefaultValue: () => false);

    var passwordLengthOption = new Option<int>(
        aliases: ["--length", "-l"],
        description: "Length of generated password",
        getDefaultValue: () => 16);

    var passwordsCountOption = new Option<int>(
        aliases: ["--count", "-c"],
        description: "Count of passwords to generate",
        getDefaultValue: () => 1);

    rootCommand.AddOption(useRussianLettersOption);
    rootCommand.AddOption(passwordLengthOption);
    rootCommand.AddOption(passwordsCountOption);

    rootCommand.SetHandler((useRussian, passwordLength, passwordsCount) =>
    {
        var lowerLetters = useRussian ? lowerLettersRussian : lowerLettersEnglish;
        var upperLetters = useRussian ? upperLettersRussian : upperLettersEnglish;
        
        for (var i = 0; i < passwordsCount; i++)
        {
            Console.WriteLine(GeneratePassword(lowerLetters, upperLetters, passwordLength));
        }
    }, useRussianLettersOption, passwordLengthOption, passwordsCountOption);

    return rootCommand;
}

static string GeneratePassword(string lowerLettersDictionary, string upperLettersDictionary, int passwordLength)
{
    var charOccurrences = CalculateCharOccurrences(passwordLength);

    var lowerLetters = GetRandomCharSpan(lowerLettersDictionary, charOccurrences.LowerLetters);
    var upperLetters = GetRandomCharSpan(upperLettersDictionary, charOccurrences.UpperLetters);
    var digits = GetRandomCharSpan(digitsDictionary, charOccurrences.Digits);
    var specialChars = GetRandomCharSpan(specialCharsDictionary, charOccurrences.SpecialChars);

    var unshuffledPasswordSpan = string.Concat(lowerLetters, upperLetters, digits, specialChars)
        .AsSpan()
        .ToReadWriteSpan();
    
    RandomNumberGenerator.Shuffle(unshuffledPasswordSpan);

    return unshuffledPasswordSpan.ToString();
}

static CharOccurrences CalculateCharOccurrences(int passwordLength)
{
    var allLettersPercent = RandomNumberGenerator.GetInt32(60, 80 + 1).ToPercent();
    var allLettersCount = (allLettersPercent * passwordLength).RoundAwayFromZero();
    
    var lowerLettersPercent = RandomNumberGenerator.GetInt32(60, 70 + 1).ToPercent() * allLettersPercent;
    var lowerLettersCount = (lowerLettersPercent * passwordLength).RoundAwayFromZero();
    
    var nonLettersPercent = 1 - allLettersPercent;
    var nonLettersCount = passwordLength - allLettersCount;

    var digitsPercent = RandomNumberGenerator.GetInt32(60, 80 + 1).ToPercent() * nonLettersPercent;
    var digitsCount = (digitsPercent * passwordLength).RoundAwayFromZero();
    
    var upperLettersCount = allLettersCount - lowerLettersCount;
    var specialCharsCount = nonLettersCount - digitsCount;

    return new CharOccurrences(lowerLettersCount, upperLettersCount, digitsCount, specialCharsCount);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
static Span<char> GetRandomCharSpan(string choices, int count)
{
    return RandomNumberGenerator.GetItems(choices.AsSpan(), count).AsSpan();
}
