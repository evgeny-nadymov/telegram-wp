// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#include "pch.h"
#include "OpenCVComponent.h"

#include <opencv2\core\core.hpp>
#include <opencv2\objdetect.hpp>
#include <opencv2\imgproc\types_c.h>
#include <opencv2\imgcodecs\imgcodecs.hpp>
#include <opencv2\imgproc\imgproc.hpp>
#include <vector>
#include <algorithm>
#include <string>

using namespace OpenCVComponent;
using namespace Platform;
using namespace concurrency;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;

// Name of the resource classifier used to detect human faces (frontal)
cv::String face_cascade_name = "haarcascade_frontalface_alt.xml";

// Name of the resource classifier used to detect human eyes (frontal)
cv::String eye_cascade_name = "haarcascade_eye.xml";

// Name of the resource classifier used to detect human mouth (frontal)
cv::String mouth_cascade_name = "haarcascade_mouth.xml";


void CopyIVectorToMatrix(IVector<int>^ input, cv::Mat& mat, int size);
void CopyMatrixToVector(const cv::Mat& mat, std::vector<int>& vector, int size);


OpenCVLib::OpenCVLib()
{
	
}

void OpenCVLib::Load()
{
	if (face_cascade.empty())
	{
		if (!face_cascade.load(face_cascade_name))
		{
			auto e = ref new Exception(-100, "Couldn't load face detector");

			throw e;
			//Windows::UI::Popups::MessageDialog("Couldn't load face detector \n").ShowAsync();
		}
	}

	if (eye_cascade.empty())
	{
		if (!eye_cascade.load(eye_cascade_name))
		{
			auto e = ref new Exception(-100, "Couldn't load eye detector");

			throw e;
			//Windows::UI::Popups::MessageDialog("Couldn't load eye detector \n").ShowAsync();
		}
	}

	if (mouth_cascade.empty())
	{
		if (!mouth_cascade.load(mouth_cascade_name))
		{
			auto e = ref new Exception(-100, "Couldn't load mouth detector");

			throw e;
			//Windows::UI::Popups::MessageDialog("Couldn't load mouth detector \n").ShowAsync();
		}
	}
}

void OutputDebugString(std::string output, LARGE_INTEGER start, LARGE_INTEGER frequency)
{
#ifdef DEBUG
	LARGE_INTEGER end;
	if (::QueryPerformanceCounter(&end) == FALSE)
		throw "foo";

	double interval = static_cast<double>(end.QuadPart - start.QuadPart) / frequency.QuadPart;

	OutputDebugStringA((output + " " + std::to_string(interval) + "\n").c_str());
#endif
}

IAsyncOperation<FacesImage^>^ OpenCVLib::ProcessImageAsync(String^ fileName)
{
	return concurrency::create_async([=]() -> FacesImage^
	{
		LARGE_INTEGER frequency;
		if (::QueryPerformanceFrequency(&frequency) == FALSE)
			throw "foo";

		LARGE_INTEGER start;
		if (::QueryPerformanceCounter(&start) == FALSE)
			throw "foo";

		Load();

		OutputDebugString("load", start, frequency);

		IVector<Face^>^ fs = ref new Platform::Collections::Vector<Face^>();

		std::wstring fooW(fileName->Begin());
		std::string fooA(fooW.begin(), fooW.end());

		cv::String fileNameCV(fooA);
		cv::Mat image = cv::imread(fileNameCV.c_str());

		OutputDebugString("cv::imread " + std::to_string(image.cols) + "x" + std::to_string(image.rows), start, frequency);

		groupFaces = cv::Mat(image.rows, image.cols, CV_8UC4);
		cv::cvtColor(image, groupFaces, CV_BGR2BGRA);

		if (!groupFaces.empty())
		{
			std::vector<cv::Rect> facesColl;
			cv::Mat frame_gray;

			OutputDebugString("start", start, frequency);

			cvtColor(groupFaces, frame_gray, CV_BGR2GRAY);
			//UpdateImage(img2, frame_gray);

			OutputDebugString("cvtColor", start, frequency);

			cv::equalizeHist(frame_gray, frame_gray);

			OutputDebugString("cv::equalizeHist", start, frequency);

			//UpdateImage(img2, frame_gray);
			// Detect faces
			face_cascade.detectMultiScale(frame_gray, facesColl, 1.2, 3, 0 | CV_HAAR_SCALE_IMAGE, cv::Size(30, 30));

			OutputDebugString("face_cascade.detectMultiScale", start, frequency);

			for (unsigned int i = 0; i < facesColl.size(); i++)
			{
				auto face = facesColl[i];
				auto f = ref new Face(Rect(face.x, face.y, face.width, face.height));
				fs->Append(f);
				
				cv::rectangle(groupFaces, face, cv::Scalar(255, 0, 0), 1);

				facesColl[i].height = facesColl[i].height / 2;
				cv::Mat faceROI = frame_gray(facesColl[i]);
				std::vector<cv::Rect> eyesColl;
				std::vector<cv::Point> eyesCenterColl;

				int eyeWidth = face.width / 6;
				eye_cascade.detectMultiScale(faceROI, eyesColl, 1.1, 3, 0 | CV_HAAR_SCALE_IMAGE, cv::Size(eyeWidth, eyeWidth));
				OutputDebugString("eye_cascade.detectMultiScale", start, frequency);
				for (unsigned int j = 0; j < eyesColl.size(); j++)
				{
					auto eye = eyesColl[j];
					eye.x += face.x;
					eye.y += face.y;
					cv::rectangle(groupFaces, eye, cv::Scalar(255, 0, 0), 1);

					auto eyeRect = Rect(eye.x, eye.y, eye.width, eye.height);
					f->Eye->Append(eyeRect);

					// eye center
					auto eyeCenter = cv::Point(eye.x + eye.width / 2, eye.y + eye.height / 2);
					eyesCenterColl.push_back(eyeCenter);
					cv::circle(groupFaces, eyeCenter, 3, cv::Scalar(0, 255, 0), -1);
				}

				std::vector<cv::Rect> mouthsColl;
				std::vector<cv::Point> mouthsCenterColl;

				facesColl[i].height = facesColl[i].height / 2;
				facesColl[i].y += facesColl[i].height * 3;
				faceROI = frame_gray(facesColl[i]);

				int mouthWidth = 30;
				mouth_cascade.detectMultiScale(faceROI, mouthsColl, 1.1, 2, 0 | CV_HAAR_SCALE_IMAGE, cv::Size(mouthWidth, mouthWidth));
				OutputDebugString("mouth_cascade.detectMultiScale", start, frequency);
				for (unsigned int j = 0; j < mouthsColl.size() && j < 10; j++)
				{
					auto mouth = mouthsColl[j];
					mouth.x += facesColl[i].x;
					mouth.y += facesColl[i].y;
					cv::rectangle(groupFaces, mouth, cv::Scalar(255, 0, 0), 1);

					auto mouthRect = Rect(mouth.x, mouth.y, mouth.width, mouth.height);
					f->Mouth->Append(mouthRect);

					auto mouthCenter = cv::Point(mouth.x + mouth.width / 2, mouth.y + mouth.height / 4);
					mouthsCenterColl.push_back(mouthCenter);
					cv::circle(groupFaces, mouthCenter, 3, cv::Scalar(0, 255, 0), -1);
				}

				// forehead, chin
				if (eyesCenterColl.size() >= 2)
				{
					auto eyesDistanceX = abs(eyesCenterColl[1].x - eyesCenterColl[0].x);
					auto eyesDistanceY = abs(eyesCenterColl[1].y - eyesCenterColl[0].y);
					auto leftEye = eyesCenterColl[0];
					if (leftEye.x > eyesCenterColl[1].x)
					{
						leftEye = eyesCenterColl[1];
					}

					auto eyesCenter = cv::Point(leftEye.x + eyesDistanceX / 2, leftEye.y + eyesDistanceY / 2);
					cv::circle(groupFaces, eyesCenter, 3, cv::Scalar(0, 255, 0), -1);

					auto foreheadCenter = cv::Point(eyesCenter.x, eyesCenter.y - 0.7 * eyesDistanceX);
					cv::circle(groupFaces, foreheadCenter, 3, cv::Scalar(0, 255, 0), -1);

					if (mouthsCenterColl.size() >= 1)
					{
						auto chinCenter = cv::Point(eyesCenter.x, mouthsCenterColl[0].y + 0.55 * eyesDistanceX);
						cv::circle(groupFaces, chinCenter, 3, cv::Scalar(0, 255, 0), -1);
					}
				}
			}

			//UpdateImage(img1, groupFaces);
		}

		std::vector<int> output;
		CopyMatrixToVector(groupFaces, output, image.rows * image.cols);

		//Return the outputs as a VectorView<float>
		//return ref new Platform::Collections::VectorView<int>(output);

		auto facesImage = ref new FacesImage();
		facesImage->Width = image.cols;
		facesImage->Height = image.rows;
		facesImage->Faces = fs;
		facesImage->Image = ref new Platform::Collections::VectorView<int>(output);


		OutputDebugString("stop", start, frequency);
		return facesImage;
	});
}

IAsyncOperation<IVectorView<int>^>^ OpenCVLib::ProcessAsync(IVector<int>^ input, int width, int height)
{
    return create_async([=]() -> IVectorView<int>^
	{
		int size = input->Size;
		cv::Mat mat(width, height, CV_8UC4);
		CopyIVectorToMatrix(input, mat, size);

        // convert to grayscale
        cv::Mat intermediateMat;
        cv::cvtColor(mat, intermediateMat, CV_RGB2GRAY);

        // convert to BGRA
        cv::cvtColor(intermediateMat, mat, CV_GRAY2BGRA);

        std::vector<int> output;
        CopyMatrixToVector(mat, output, size);

        // Return the outputs as a VectorView<float>
        return ref new Platform::Collections::VectorView<int>(output);
    });
}


void CopyIVectorToMatrix(IVector<int>^ input, cv::Mat& mat, int size)
{
    unsigned char* data = mat.data;
    for (int i = 0; i < size; i++)
    {
        int value = input->GetAt(i);
        memcpy(data, (void*) &value, 4);
        data += 4;
    }
}

void CopyMatrixToVector(const cv::Mat& mat, std::vector<int>& vector, int size)
{
    int* data = (int*) mat.data;
    for (int i = 0; i < size; i++)
    {
        vector.push_back(data[i]);
    }

}