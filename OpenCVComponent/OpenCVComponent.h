// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#pragma once

#include <ppltasks.h>
#include <collection.h>

#include <opencv2\objdetect.hpp>
#include <opencv2\core\core.hpp>

namespace OpenCVComponent
{
	using namespace Windows::Foundation;
	using namespace Windows::Foundation::Collections;
	using namespace Platform;

	public ref class Face sealed
	{
	private:
	public:
		property IVector<Rect>^ Eye;
		property IVector<Rect>^ Mouth;
		property Rect Position;

		Face(Rect position)
		{
			Eye = ref new Platform::Collections::Vector<Rect>();
			Mouth = ref new Platform::Collections::Vector<Rect>();
			Position = position;
		}
	};

	public ref class FacesImage sealed
	{
	public:
		property int Width;
		property int Height;
		property IVectorView<int>^ Image;
		property IVector<Face^>^ Faces;
	};

    public ref class OpenCVLib sealed
    {
	private:
		cv::Mat groupFaces;
		cv::CascadeClassifier face_cascade;
		cv::CascadeClassifier eye_cascade;
		cv::CascadeClassifier mouth_cascade;
		void Load();
    public:
        OpenCVLib();
		IAsyncOperation<FacesImage^>^ ProcessImageAsync(String^ fileName);
        IAsyncOperation<IVectorView<int>^>^ ProcessAsync(IVector<int>^ input, int width, int height);
    };
}