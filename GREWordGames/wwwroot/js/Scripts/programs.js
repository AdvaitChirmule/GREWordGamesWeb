function HowOld(dateDifference)
{
    differenceString = ''
    year = parseInt(dateDifference.slice(0, 4)) - 1970
    month = parseInt(dateDifference.slice(5, 7)) - 1
    day = parseInt(dateDifference.slice(8, 10)) - 1
    hours = parseInt(dateDifference.slice(11, 13))
    minutes = parseInt(dateDifference.slice(14, 16))
    if (year > 0) {
        if (year == 1) {
            differenceString = "1 year ago"
        }
        else {
            differenceString = year.toString() + " years ago"
        }
    }
    else if (month > 0) {
        if (month == 1) {
            differenceString = "1 month ago"
        }
        else {
            differenceString = month.toString() + " months ago"
        }
    }
    else if (day > 0) {
        if (day == 1) {
            differenceString = "1 day ago"
        }
        else {
            differenceString = day.toString() + " days ago"
        }
    }
    else if (hours > 0) {
        if (hours == 1) {
            differenceString = "1 hour ago"
        }
        else {
            differenceString = hours.toString() + " days ago"
        }
    }
    else if (minutes > 0) {
        if (minutes == 1) {
            differenceString = "1 minute ago"
        }
        else {
            differenceString = minutes.toString() + " minutes ago"
        }
    }
    else {
        differenceString = "now"
    }

    return differenceString
}

function ConvertToProficiency(inputString) {
    proficiency = inputString.slice(1, inputString.length - 1).split("|");
    var output = 0;
    if (parseInt(proficiency[1]) == 0) {
        output = 0;
    }
    else {
        output = parseFloat(100 * parseInt(proficiency[0]) / parseInt(proficiency[1])).toFixed(2);
    }
    return output;
}

function CapitalizeFirstLetter(word) {
    return String(word).charAt(0).toUpperCase() + String(word).slice(1);
}

function SortByRow(j, wordIndexes, reverse) {

    rowSorted = wordIndexes[j].slice();

    if (j == 0) {
        rowSorted.sort((a, b) => a - b);
    }
    else {
        rowSorted.sort();
    }
    
    if (reverse == true) {
        rowSorted.reverse();
    }
    var ogRowHash = {};
    for (let i = 0; i < wordIndexes[j].length; i++) {
        if (wordIndexes[j][i] in ogRowHash) {
            ogRowHash[wordIndexes[j][i]].push(i);
        }
        else {
            ogRowHash[wordIndexes[j][i]] = [i];
        }
    }

    var rowHash = {};
    for (let i = 0; i < rowSorted.length; i++) {
        if (rowSorted[i] in rowHash) {
            rowHash[rowSorted[i]].push(i);
        }
        else {
            rowHash[rowSorted[i]] = [i];
        }
    }

    var newHash = {};
    for (let i = 0; i < rowSorted.length; i++) {
        for (let j = 0; j < rowHash[rowSorted[i]].length; j++) {
            newHash[rowHash[rowSorted[i]][j]] = ogRowHash[rowSorted[i]][j];
        }
    }

    newWordIndexes = [[], [], [], []];
    for (let i = 0; i < wordIndexes[0].length; i++) {
        newWordIndexes[0].push(wordIndexes[0][newHash[i]]);
        newWordIndexes[1].push(wordIndexes[1][newHash[i]]);
        newWordIndexes[2].push(wordIndexes[2][newHash[i]]);
        newWordIndexes[3].push(wordIndexes[3][newHash[i]]);
    }

    return newWordIndexes;
}