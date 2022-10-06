#pragma once

/************************************************************************
* RSA 28/11/21
* https://ftp.ebi.ac.uk/pub/databases/emdb/doc/Map-format/current/EMDB_map_format.pdf
************************************************************************/

#include <string>
#include <vector>
#include <map>
#include "MatrixThreeThree.h"

using namespace std;

class Ccp4File
{
private:

public:	
	int W01_NC;
	int W02_NR;
	int W03_NS;
	int W04_Mode;
	int W05_NCSTART;
	int W06_NRSTART;
	int W07_NSSTART;
	int W08_NX;
	int W09_NY;
	int W10_NZ;
	double W11_CELLA_X;
	double W12_CELLA_Y;
	double W13_CELLA_Z;
	double W14_CELLB_X;
	double W15_CELLB_Y;
	double W16_CELLB_Z;
	int W17_MAPC;
	int W18_MAPR;
	int W19_MAPS;

private:	
	double PI;


	float _w22_DMEAN;

	//Calculation data
	MatrixThreeThree _orthoMat;
	MatrixThreeThree _deOrthoMat;
	vector<int> _map2xyz;
	vector<int> _map2crs;
	vector<float>_cellDims;
	vector<int> _axisSampling;
	vector<int> _crsStart;
	vector<int> _dimOrder;
	VectorThree _origin;


	//Helper functioms	
	void calculateOrthoMat(float w11_CELLA_X, float w12_CELLA_Y, float w13_CELLA_Z, float w14_CELLB_X, float w15_CELLB_Y, float w16_CELLB_Z);
	void calculateOrigin(int w05_NXSTART, int w06_NYSTART, int w07_NZSTART, int w17_MAPC, int w18_MAPR, int w19_MAPS);

public:
	//The matrix data lazily as a public accessor
	vector<double> Matrix;

public:
	Ccp4File(vector<double> matrix);
	VectorThree getCRS(int position);
	void setWords(
		int w01_NC,
		int w02_NR,
		int w03_NS,
		int w04_Mode,
		int w05_NCSTART,
		int w06_NRSTART,
		int w07_NSSTART,
		int w08_NX,
		int w09_NY,
		int w10_NZ,
		double w11_CELLA_X,
		double w12_CELLA_Y,
		double w13_CELLA_Z,
		double w14_CELLB_X,
		double w15_CELLB_Y,
		double w16_CELLB_Z,
		int w17_MAPC,
		int w18_MAPR,
		int w19_MAPS);

	double getResolution();
	bool isLoaded();
	string getPdbCode();
	float getDensity(int C, int R, int S);
	//VectorThree getNearestPeakOld(VectorThree XYZ, Interpolator* interp, int interpNum);
	VectorThree getCRSFromXYZ(VectorThree XYZ);
	VectorThree getXYZFromCRS(VectorThree CRS);
	int getPosition(int C, int R, int S);

private:
	void loadInfo();
	//void createWordsData(string ccp4File);
	//void createWords(string ccp4File);
	//void createWordsList(int symmetry, int length, int nCnRnS);


public:	
	
	
	
};



