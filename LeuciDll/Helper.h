#pragma once
/************************************************************************
* RSA 9.9.21
************************************************************************/

#include <string>
#include <vector>

using namespace std;

class Helper
{
public:
    static vector<string> stringToVector(string input, string delim);
    static string getNumberStringGaps(double number, int dp, int length);
    static string getWordStringGaps(string word, int length);
};