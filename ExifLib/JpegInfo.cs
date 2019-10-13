using System;

namespace ExifLib
{
    public class JpegInfo
    {
        /// <summary>
        /// The Jpeg file name (excluding path).
        /// </summary>
        public string FileName;
        /// <summary>
        /// The Jpeg file size, in bytes.
        /// </summary>
        public int FileSize;
        /// <summary>
        /// True if the provided Stream was detected to be a Jpeg image, False otherwise.
        /// </summary>
        public bool IsValid;

        /// <summary>
        /// Image dimensions, in pixels.
        /// </summary>
        public int Width, Height;
        /// <summary>
        /// True if the image data is expressed in 3 components (RGB), False otherwise.
        /// </summary>
        public bool IsColor;

        /// <summary>
        /// Orientation of the image.
        /// </summary>
        public ExifOrientation Orientation;
        /// <summary>
        /// The X and Y resolutions of the image, expressed in ResolutionUnit.
        /// </summary>
        public double XResolution, YResolution;
        /// <summary>
        /// Resolution unit of the image.
        /// </summary>
        public ExifUnit ResolutionUnit;

        /// <summary>
        /// Date at which the image was taken.
        /// </summary>
        public string DateTime;
        /// <summary>
        /// Description of the image.
        /// </summary>
        public string Description;
        /// <summary>
        /// Camera manufacturer.
        /// </summary>
        public string Make;
        /// <summary>
        /// Camera model.
        /// </summary>
        public string Model;
        /// <summary>
        /// Software used to create the image.
        /// </summary>
        public string Software;
        /// <summary>
        /// Image artist.
        /// </summary>
        public string Artist;
        /// <summary>
        /// Image copyright.
        /// </summary>
        public string Copyright;
        /// <summary>
        /// Image user comments.
        /// </summary>
        public string UserComment;
        /// <summary>
        /// Exposure time, in seconds.
        /// </summary>
        public double ExposureTime;
        /// <summary>
        /// F-number (F-stop) of the camera lens when the image was taken.
        /// </summary>
        public double FNumber;
        /// <summary>
        /// Flash settings of the camera when the image was taken.
        /// </summary>
        public ExifFlash Flash;

        /// <summary>
        /// GPS latitude reference (North, South).
        /// </summary>
        public ExifGpsLatitudeRef GpsLatitudeRef;
        /// <summary>
        /// GPS latitude (degrees, minutes, seconds).
        /// </summary>
        public double[] GpsLatitude = new double[3];
        /// <summary>
        /// GPS longitude reference (East, West).
        /// </summary>
        public ExifGpsLongitudeRef GpsLongitudeRef;
        /// <summary>
        /// GPS longitude (degrees, minutes, seconds).
        /// </summary>
        public double[] GpsLongitude = new double[3];

        /// <summary>
        /// Byte offset and size of the thumbnail data within the Exif section of the image file.
        /// Used internally.
        /// </summary>
        public int ThumbnailOffset, ThumbnailSize;
        /// <summary>
        /// Thumbnail data found in the Exif section.
        /// </summary>
        public byte[] ThumbnailData;
        
        /// <summary>
        /// Time taken to load the image information.
        /// </summary>
        public TimeSpan LoadTime;
    }
}
