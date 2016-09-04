// Luke Holbert
// Astro 581
// Homework 5

#define _USE_MATH_DEFINES
#include <stdio.h>
#include <cmath>
#include <fstream>
using std::ofstream;
using std::endl;

double maxSoundSpeed(double *U1, double *U2, double *U3)
{
	double maxSpeed = 0;
	double speed = 0;
	double pressure = 0;
	double density = 0;
	double soundSpeed = 0;
	int index = 0;

	while (index <= 400)
	{
		if (U1[index] != 0)
		{
			density = U1[index];
			speed = U2[index] / density;
			pressure = (U3[index] - density * speed * speed * 0.5) * (0.4);
			soundSpeed = sqrt(1.4 * abs(pressure) / density);

			if (maxSpeed < soundSpeed + abs(speed))
			{
				maxSpeed = soundSpeed + abs(speed);
			}
		}

		index++;
	}

	return maxSpeed;
}

double maxLeftSpeed(double *U1, double *U2, double *U3)
{
	double maxSpeed = 0;
	double speed = 0;
	double pressure = 0;
	double density = 0;
	double soundSpeed = 0;
	int index = 0;

	while (index <= 400)
	{
		if (U1[index] != 0)
		{
			density = U1[index];
			speed = U2[index] / density;
			pressure = (U3[index] - density * speed * speed * 0.5) * (0.4);
			soundSpeed = sqrt(1.4 * abs(pressure) / density);

			if (maxSpeed > speed - soundSpeed)
			{
				maxSpeed = speed - soundSpeed;
			}
		}

		index++;
	}

	return maxSpeed;
}

double maxRightSpeed(double *U1, double *U2, double *U3)
{
	double maxSpeed = 0;
	double speed = 0;
	double pressure = 0;
	double density = 0;
	double soundSpeed = 0;
	int index = 0;

	while (index <= 400)
	{
		if (U1[index] != 0)
		{
			density = U1[index];
			speed = U2[index] / density;
			pressure = (U3[index] - density * speed * speed * 0.5) * (0.4);
			soundSpeed = sqrt(1.4 * abs(pressure) / density);

			if (maxSpeed < soundSpeed + speed)
			{
				maxSpeed = soundSpeed + speed;
			}
		}

		index++;
	}

	return maxSpeed;
}

// HLLE Routine
void HLLE(double *U1, double *U2, double *U3, int index, double *F1, double *F2, double *F3)
{
	double Sl = maxLeftSpeed(U1, U2, U3);
	double Sr = maxRightSpeed(U1, U2, U3);
	double F1l = U2[index];
	double F2l = U2[index] * U2[index] / U1[index] + (1.4 - 1.0) * (U3[index] - U2[index] * U2[index] / U1[index] / 2);
	double F3l = U2[index] / U1[index] * (U3[index] + (1.4 - 1.0) * (U3[index] - U2[index] * U2[index] / U1[index] / 2));
	double F1r = U2[index + 1];
	double F2r = U2[index + 1] * U2[index + 1] / U1[index + 1] + (1.4 - 1.0) * (U3[index + 1] - U2[index + 1] * U2[index + 1] / U1[index + 1] / 2);
	double F3r = U2[index + 1] / U1[index + 1] * (U3[index + 1] + (1.4 - 1.0) * (U3[index + 1] - U2[index + 1] * U2[index + 1] / U1[index + 1] / 2));

	if (Sl >= 0)
	{
		F1[index] = F1l;
		F2[index] = F2l;
		F3[index] = F3l;
	}
	else if (Sr <= 0)
	{
		F1[index] = F1r;
		F2[index] = F2r;
		F3[index] = F3r;
	}
	else
	{
		F1[index] = (Sr * F1l - Sl * F1r + Sl * Sr * (U1[index + 1] - U1[index])) / (Sr - Sl);
		F2[index] = (Sr * F2l - Sl * F2r + Sl * Sr * (U2[index + 1] - U2[index])) / (Sr - Sl);
		F3[index] = (Sr * F3l - Sl * F3r + Sl * Sr * (U3[index + 1] - U3[index])) / (Sr - Sl);
	}
}

int main(void)
{
	int xIndex = 0;
	int index = 0;
	double tValue = 0.0;
	double xValue = -.5;
	double h = .0025;
	double CFL = .1;
	double timestep = 0.0;
	double C = 0.0;
	double gamma = 1.4;

	double U1[401] = { 0 }; // density
	double U2[401] = { 0 }; // density * velocity
	double U3[401] = { 0 }; // energy per volume
	double F1[401] = { 0 }; // density * velocity
	double F2[401] = { 0 }; // density * (velocity ^ 2) + Pressure -------> Pressure = (gamma - 1) * (energy - density * (velocity ^ 2) / 2)
	double F3[401] = { 0 }; // velocity * (energy + Pressure)
	ofstream initFile;
	ofstream dataFile;

	dataFile.open("data1.txt", ofstream::out);
	initFile.open("init.txt", ofstream::out);

	// initial function arrays
	while (xIndex <= 400)
	{
		if (xValue < C)
		{
			U1[xIndex] = 1.0;
			U2[xIndex] = 0;
			U3[xIndex] = 1.0 / (gamma - 1.0);
		}
		else
		{
			U1[xIndex] = 0.125;
			U2[xIndex] = 0;
			U3[xIndex] = .1 / (gamma - 1.0);
		}
		xValue += h;
		xIndex++;
	}

	xValue = -.5;
	xIndex = 0;

	initFile << "xValue    Pressure    Density    Velocity" << endl;
	dataFile << "xValue    Pressure    Density    Velocity" << endl;

	// print out inital solution
	while (xIndex <= 400)
	{
		initFile << xValue << "   " << U3[xIndex] * (gamma - 1.0) << "   " << U1[xIndex] << "   " << U2[xIndex] << endl;
		xValue += h;
		xIndex++;
	}

	xIndex = 0;
	tValue = 0;

	// Godunov and HLLE
	// Change to tValue < 1 for second set of graphs
	while (tValue < .2)
	{
		timestep = CFL * h / maxSoundSpeed(U1, U2, U3);

		// Find flux value
		while (xIndex < 400)
		{
			HLLE(U1, U2, U3, xIndex, F1, F2, F3);
			xIndex++;
		}

		xIndex = 1;

		while (xIndex < 400)
		{
			U1[xIndex] -= timestep / h * (F1[xIndex] - F1[xIndex - 1]);
			U2[xIndex] -= timestep / h * (F2[xIndex] - F2[xIndex - 1]);
			U3[xIndex] -= timestep / h * (F3[xIndex] - F3[xIndex - 1]);
			xIndex++;
		}

		U1[0] = U1[1];
		U2[0] = U2[1];
		U3[0] = U3[1];
		U1[400] = U1[399];
		U2[400] = U2[399];
		U3[400] = U3[399];

		xIndex = 0;
		tValue += timestep;
	}

	xIndex = 0;
	xValue = -.5;
	// print out final solution
	while (xIndex <= 400)
	{
		dataFile << xValue << "   " << (U3[xIndex] - (U1[xIndex]) * (U2[xIndex] / U1[xIndex]) * (U2[xIndex] / U1[xIndex]) / 2) * (gamma - 1.0) << "   " << U1[xIndex] << "   " << U2[xIndex] / U1[xIndex] << endl;
		xValue += h;
		xIndex++;
	}

	initFile.close();
	dataFile.close();

	return 0;
}