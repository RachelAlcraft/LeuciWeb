// MathLibrary.h - Contains declarations of math functions
#pragma once

#include <vector>


#ifdef DENSITYMATRIX_EXPORTS
#define DENSITYMATRIX_API __declspec(dllexport)
#else
#define DENSITYMATRIX_API __declspec(dllimport)
#endif

// The Fibonacci recurrence relation describes a sequence F
// where F(n) is { n = 0, a
//               { n = 1, b
//               { n > 1, F(n-2) + F(n-1)
// for some initial integral values a and b.
// If the sequence is initialized F(0) = 1, F(1) = 1,
// then this relation produces the well-known Fibonacci
// sequence: 1, 1, 2, 3, 5, 8, 13, 21, 34, ...

// Initialize a Fibonacci relation sequence
// such that F(0) = a, F(1) = b.
// This function must be called before any other function.
extern "C" DENSITYMATRIX_API bool density_init(	
	const char* pdbcode,
	const char* filepath);

extern "C" DENSITYMATRIX_API void density_add(
	double val);

extern "C" DENSITYMATRIX_API void density_words(
	int w01_NC, int w02_NR, int w03_NS, int w04_Mode,
	int w05_NCSTART, int w06_NRSTART, int w07_NSSTART,
	int w08_NX, int w09_NY, int w10_NZ,
	double w11_CELLA_X, double w12_CELLA_Y, double w13_CELLA_Z,
	double w14_CELLB_X, double w15_CELLB_Y, double w16_CELLB_Z,
	int w17_MAPC, int w18_MAPR, int w19_MAPS);

extern "C" DENSITYMATRIX_API void density_calc();

extern "C" DENSITYMATRIX_API void create_slice(
	double cx, 
	double cy, 
	double cz, 
	double lx, 
	double ly, 
	double lz, 
	double px, 
	double py, 
	double pz, 
	double width, 
	double gap);

extern "C" DENSITYMATRIX_API double get_slice_value(
	unsigned int a, 
	unsigned int b);

extern "C" DENSITYMATRIX_API double get_slice_radiant_value(
	unsigned int a,
	unsigned int b);

extern "C" DENSITYMATRIX_API double get_slice_laplacian_value(
	unsigned int a,
	unsigned int b);

