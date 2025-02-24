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