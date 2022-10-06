#pragma once


/************************************************************************
* RSA 6.9.21
************************************************************************/

#include <string>
#include <vector>

#include "VectorThree.h"

using namespace std;

class MatrixThreeThree
{
private:
	vector<double> _matrix;



public:
	MatrixThreeThree();
	MatrixThreeThree(vector<double> vals);
	MatrixThreeThree getInverse();
	double getDeterminant();
	double getInnerDeterminant(int col, int row);
	double getValue(int col, int row);
	void putValue(double val, int col, int row);
	VectorThree multiply(VectorThree col, bool byRow);

};

