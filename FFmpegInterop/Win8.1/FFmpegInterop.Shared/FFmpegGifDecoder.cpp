#include "pch.h"
#include "FFmpegGifDecoder.h"
#include <cstdint>
#include <limits>
#include <ctime>

#ifndef NOMINMAX
#undef min
#undef max
#endif

extern "C"
{
#include <libavformat/avformat.h>
}
using namespace FFmpegInterop;

typedef struct VideoInfo {

	~VideoInfo() {
		if (video_dec_ctx) {
			avcodec_close(video_dec_ctx);
			video_dec_ctx = nullptr;
		}
		if (fmt_ctx) {
			avformat_close_input(&fmt_ctx);
			fmt_ctx = nullptr;
		}
		if (frame) {
			av_frame_free(&frame);
			frame = nullptr;
		}
		if (src) {
			delete[] src;
			src = nullptr;
		}
		if (free_orig_pkt)
		{
			av_free_packet(&orig_pkt);
			free_orig_pkt = false;
		}
		//av_packet_unref(&orig_pkt);

		video_stream_idx = -1;
		video_stream = nullptr;
	}

	AVFormatContext *fmt_ctx = nullptr;
	char *src = nullptr;
	int video_stream_idx = -1;
	AVStream *video_stream = nullptr;
	AVCodecContext *video_dec_ctx = nullptr;
	AVFrame *frame = nullptr;
	bool has_decoded_frames = false;
	bool free_orig_pkt = false;
	AVPacket pkt;
	AVPacket orig_pkt;
};

void Log(Platform::String^ message, Platform::Object^ parameter1, Platform::Object^ parameter2){
	auto para1String = parameter1->ToString();
	auto para2String = parameter2->ToString();
	auto msg = std::wstring(message->Data());
	auto para1 = std::wstring(safe_cast<Platform::String^>(para1String)->Data());
	auto para2 = std::wstring(safe_cast<Platform::String^>(para2String)->Data());

	auto offset1 = msg.find(L"{0}");

	auto formattedText = msg.replace(offset1, 3, para1);// .append(L"\r\n");

	auto offset2 = formattedText.find(L"{1}");

	formattedText = formattedText.replace(offset2, 3, para2).append(L"\r\n");

	::OutputDebugString(formattedText.c_str());
}

void Log(Platform::String^ message, Platform::Object^ parameter){
	auto paraString = parameter->ToString();
	auto msg = std::wstring(message->Data());
	auto para = std::wstring(safe_cast<Platform::String^>(paraString)->Data());

	auto offset = msg.find(L"{0}");

	auto formattedText = msg.replace(offset, 3, para).append(L"\r\n");

	::OutputDebugString(formattedText.c_str());
}

int open_codec_context(int *stream_idx, AVFormatContext *fmt_ctx, enum AVMediaType type) {
	int ret;
	AVStream *st;
	AVCodecContext *dec_ctx = NULL;
	AVCodec *dec = NULL;
	AVDictionary *opts = NULL;

	ret = av_find_best_stream(fmt_ctx, type, -1, -1, NULL, 0);
	if (ret < 0) {
		//LOGE("can't find %s stream in input file\n", av_get_media_type_string(type));
		return ret;
	}
	else {
		*stream_idx = ret;
		st = fmt_ctx->streams[*stream_idx];

		dec_ctx = st->codec;
		dec = avcodec_find_decoder(dec_ctx->codec_id);
		if (!dec) {
			//LOGE("failed to find %s codec\n", av_get_media_type_string(type));
			return ret;
		}

		av_dict_set(&opts, "refcounted_frames", "1", 0);
		if ((ret = avcodec_open2(dec_ctx, dec, &opts)) < 0) {
			//LOGE("failed to open %s codec\n", av_get_media_type_string(type));
			return ret;
		}
	}

	return 0;
}

int decode_packet(VideoInfo *info, int *got_frame) {
	try{

		int ret = 0;
		int decoded = info->pkt.size;

		*got_frame = 0;
		if (info->pkt.stream_index == info->video_stream_idx) {
			ret = avcodec_decode_video2(info->video_dec_ctx, info->frame, got_frame, &info->pkt);
			if (ret != 0) {
				return ret;
			}
		}

		return decoded;
	}
	catch (Platform::Exception^ e)
	{

	}

	return -1;
}

static bool _once;

int FFmpegGifDecoder::CreateDecoder(Platform::String^ src, Platform::WriteOnlyArray<int>^ data)
{
	//if (!_once)
	{
		_once = true;
		av_register_all();
	}

	VideoInfo *info = new VideoInfo();

	Platform::String^ fooRT = src;
	std::wstring fooW(fooRT->Begin());
	std::string fooA(fooW.begin(), fooW.end());
	const char* srcString = fooA.c_str();

	int len = strlen(srcString);
	info->src = new char[len + 1];
	memcpy(info->src, srcString, len);
	info->src[len] = '\0';
	if (srcString != 0) {
		//delete[] srcString;
		//env->ReleaseStringUTFChars(src, srcString);
	}

	int ret;
	if ((ret = avformat_open_input(&info->fmt_ctx, info->src, NULL, NULL)) < 0) {
		//LOGE("can't open source file %s, %s", info->src, av_err2str(ret));
		char* error = new char[256];
		auto str = av_strerror(ret, error, 256);
		delete info;
		return 0;
	}

	if ((ret = avformat_find_stream_info(info->fmt_ctx, NULL)) < 0) {
		//LOGE("can't find stream information %s, %s", info->src, av_err2str(ret));
		delete info;
		return 0;
	}

	if (open_codec_context(&info->video_stream_idx, info->fmt_ctx, AVMEDIA_TYPE_VIDEO) >= 0) {
		info->video_stream = info->fmt_ctx->streams[info->video_stream_idx];
		info->video_dec_ctx = info->video_stream->codec;
	}

	if (info->video_stream <= 0) {
		//LOGE("can't find video stream in the input, aborting %s", info->src);
		delete info;
		return 0;
	}

	info->frame = av_frame_alloc();
	if (info->frame == nullptr) {
		//LOGE("can't allocate frame %s", info->src);
		delete info;
		return 0;
	}

	av_init_packet(&info->pkt);
	info->pkt.data = NULL;
	info->pkt.size = 0;
	info->orig_pkt = info->pkt;

	//jint *dataArr = env->GetIntArrayElements(data, 0);
	if (data != nullptr && data->Length >= 2) {
		data[0] = info->video_dec_ctx->width;
		data[1] = info->video_dec_ctx->height;
		//env->ReleaseIntArrayElements(data, dataArr, 0);
	}

	//LOGD("successfully opened file %s", info->src);

	return (int)info;
}

void FFmpegGifDecoder::DestroyDecoder(int ptr) 
{
	if (ptr == NULL) {
		return;
	}
	
	VideoInfo *info = (VideoInfo *)ptr;
	delete info;
}

#define USE_BRANCHLESS 0
#if USE_BRANCHLESS
static __inline int32 clamp0(int32 v) {
	return ((-(v) >> 31) & (v));
}

static __inline int32 clamp255(int32 v) {
	return (((255 - (v)) >> 31) | (v)) & 255;
}

static __inline uint32 Clamp(int32 val) {
	int v = clamp0(val);
	return (uint32)(clamp255(v));
}

static __inline uint32 Abs(int32 v) {
	int m = v >> 31;
	return (v + m) ^ m;
}
#else  // USE_BRANCHLESS
static __inline int32 clamp0(int32 v) {
	return (v < 0) ? 0 : v;
}

static __inline int32 clamp255(int32 v) {
	return (v > 255) ? 255 : v;
}

static __inline uint32 Clamp(int32 val) {
	int v = clamp0(val);
	return (uint32)(clamp255(v));
}

static __inline uint32 Abs(int32 v) {
	return (v < 0) ? -v : v;
}
#endif  // USE_BRANCHLESS

#define YG 74 /* (int8)(1.164 * 64 + 0.5) */

#define UB 127 /* min(63,(int8)(2.018 * 64)) */
#define UG -25 /* (int8)(-0.391 * 64 - 0.5) */
#define UR 0

#define VB 0
#define VG -52 /* (int8)(-0.813 * 64 - 0.5) */
#define VR 102 /* (int8)(1.596 * 64 + 0.5) */

// Bias
#define BB UB * 128 + VB * 128
#define BG UG * 128 + VG * 128
#define BR UR * 128 + VR * 128

static __inline void YuvPixel(uint8 y, uint8 u, uint8 v,
	uint8* b, uint8* g, uint8* r) {
	int32 y1 = ((int32)(y)-16) * YG;
	*b = Clamp((int32)((u * UB + v * VB) - (BB)+y1) >> 6);
	*g = Clamp((int32)((u * UG + v * VG) - (BG)+y1) >> 6);
	*r = Clamp((int32)((u * UR + v * VR) - (BR)+y1) >> 6);
}

// Also used for 420
void I422ToARGBRow_C(const uint8* src_y,
	const uint8* src_u,
	const uint8* src_v,
	uint8* rgb_buf,
	int width) {
	int x;
	for (x = 0; x < width - 1; x += 2) {
		YuvPixel(src_y[0], src_u[0], src_v[0],
			rgb_buf + 0, rgb_buf + 1, rgb_buf + 2);
		rgb_buf[3] = 255;
		YuvPixel(src_y[1], src_u[0], src_v[0],
			rgb_buf + 4, rgb_buf + 5, rgb_buf + 6);
		rgb_buf[7] = 255;
		src_y += 2;
		src_u += 1;
		src_v += 1;
		rgb_buf += 8;  // Advance 2 pixels.
	}
	if (width & 1) {
		YuvPixel(src_y[0], src_u[0], src_v[0],
			rgb_buf + 0, rgb_buf + 1, rgb_buf + 2);
		rgb_buf[3] = 255;
	}
}

// Convert I420 to ARGB.
// LIBYUV_API
int I420ToARGB(const uint8* src_y, int src_stride_y,
	const uint8* src_u, int src_stride_u,
	const uint8* src_v, int src_stride_v,
	uint8* dst_argb, int dst_stride_argb,
	int width, int height) {
	if (!src_y || !src_u || !src_v || !dst_argb ||
		width <= 0 || height == 0) {
		return -1;
	}
	// Negative height means invert the image.
	if (height < 0) {
		height = -height;
		dst_argb = dst_argb + (height - 1) * dst_stride_argb;
		dst_stride_argb = -dst_stride_argb;
	}
	void(*I422ToARGBRow)(const uint8* y_buf,
		const uint8* u_buf,
		const uint8* v_buf,
		uint8* rgb_buf,
		int width) = I422ToARGBRow_C;
#if defined(HAS_I422TOARGBROW_SSSE3)
	if (TestCpuFlag(kCpuHasSSSE3) && width >= 8) {
		I422ToARGBRow = I422ToARGBRow_Any_SSSE3;
		if (IS_ALIGNED(width, 8)) {
			I422ToARGBRow = I422ToARGBRow_Unaligned_SSSE3;
			if (IS_ALIGNED(dst_argb, 16) && IS_ALIGNED(dst_stride_argb, 16)) {
				I422ToARGBRow = I422ToARGBRow_SSSE3;
			}
		}
	}
#endif
#if defined(HAS_I422TOARGBROW_AVX2)
	if (TestCpuFlag(kCpuHasAVX2) && width >= 16) {
		I422ToARGBRow = I422ToARGBRow_Any_AVX2;
		if (IS_ALIGNED(width, 16)) {
			I422ToARGBRow = I422ToARGBRow_AVX2;
		}
	}
#endif
#if defined(HAS_I422TOARGBROW_NEON)
	if (TestCpuFlag(kCpuHasNEON) && width >= 8) {
		I422ToARGBRow = I422ToARGBRow_Any_NEON;
		if (IS_ALIGNED(width, 8)) {
			I422ToARGBRow = I422ToARGBRow_NEON;
		}
	}
#endif
#if defined(HAS_I422TOARGBROW_MIPS_DSPR2)
	if (TestCpuFlag(kCpuHasMIPS_DSPR2) && IS_ALIGNED(width, 4) &&
		IS_ALIGNED(src_y, 4) && IS_ALIGNED(src_stride_y, 4) &&
		IS_ALIGNED(src_u, 2) && IS_ALIGNED(src_stride_u, 2) &&
		IS_ALIGNED(src_v, 2) && IS_ALIGNED(src_stride_v, 2) &&
		IS_ALIGNED(dst_argb, 4) && IS_ALIGNED(dst_stride_argb, 4)) {
		I422ToARGBRow = I422ToARGBRow_MIPS_DSPR2;
	}
#endif

	for (int y = 0; y < height; ++y) {
		I422ToARGBRow(src_y, src_u, src_v, dst_argb, width);
		dst_argb += dst_stride_argb;
		src_y += src_stride_y;
		if (y & 1) {
			src_u += src_stride_u;
			src_v += src_stride_v;
		}
	}
	return 0;
}


// Use first 4 shuffler values to reorder ARGB channels.
void ARGBShuffleRow_C(const uint8* src_argb, uint8* dst_argb,
	const uint8* shuffler, int pix) {
	int index0 = shuffler[0];
	int index1 = shuffler[1];
	int index2 = shuffler[2];
	int index3 = shuffler[3];
	// Shuffle a row of ARGB.
	int x;
	for (x = 0; x < pix; ++x) {
		// To support in-place conversion.
		uint8 b = src_argb[index0];
		uint8 g = src_argb[index1];
		uint8 r = src_argb[index2];
		uint8 a = src_argb[index3];
		dst_argb[0] = b;
		dst_argb[1] = g;
		dst_argb[2] = r;
		dst_argb[3] = a;
		src_argb += 4;
		dst_argb += 4;
	}
}

int ARGBShuffle(const uint8* src_bgra, int src_stride_bgra,
	uint8* dst_argb, int dst_stride_argb,
	const uint8* shuffler, int width, int height) {
	int y;
	void(*ARGBShuffleRow)(const uint8* src_bgra, uint8* dst_argb,
		const uint8* shuffler, int pix) = ARGBShuffleRow_C;
	if (!src_bgra || !dst_argb ||
		width <= 0 || height == 0) {
		return -1;
	}
	// Negative height means invert the image.
	if (height < 0) {
		height = -height;
		src_bgra = src_bgra + (height - 1) * src_stride_bgra;
		src_stride_bgra = -src_stride_bgra;
	}
	// Coalesce rows.
	if (src_stride_bgra == width * 4 &&
		dst_stride_argb == width * 4) {
		width *= height;
		height = 1;
		src_stride_bgra = dst_stride_argb = 0;
	}

	for (y = 0; y < height; ++y) {
		ARGBShuffleRow(src_bgra, dst_argb, shuffler, width);
		src_bgra += src_stride_bgra;
		dst_argb += dst_stride_argb;
	}
	return 0;
}


// Shuffle table for converting ABGR to ARGB.
static uint8 kShuffleMaskABGRToARGB[16] = {
	2u, 1u, 0u, 3u, 6u, 5u, 4u, 7u, 10u, 9u, 8u, 11u, 14u, 13u, 12u, 15u
};

// Convert ABGR to ARGB.
int ABGRToARGB(const uint8* src_abgr, int src_stride_abgr,
uint8* dst_argb, int dst_stride_argb,
int width, int height) {
	return ARGBShuffle(src_abgr, src_stride_abgr,
		dst_argb, dst_stride_argb,
		(const uint8*)(&kShuffleMaskABGRToARGB),
		width, height);
}

Platform::Array<uint8_t>^ FFmpegGifDecoder::GetVideoFrame(int ptr, Platform::WriteOnlyArray<int>^ data) {
	auto emptyArray = ref new Platform::Array<uint8_t>(0);
	unsigned int start = clock();

	//Log("start={0}", start);
	if (ptr == NULL) {
		return emptyArray;
	}
	VideoInfo *info = (VideoInfo *)ptr;
	int ret = 0;
	int got_frame = 0;

	while (true) {
		if (info->pkt.size == 0) {
			ret = av_read_frame(info->fmt_ctx, &info->pkt);
			//LOGD("got packet with size %d", info->pkt.size);
			if (ret >= 0) {
				info->orig_pkt = info->pkt;
				info->free_orig_pkt = true;
			}
		}

		if (info->pkt.size > 0) {
			ret = decode_packet(info, &got_frame);
			if (ret < 0) {
				if (info->has_decoded_frames) {
					ret = 0;
				}
				info->pkt.size = 0;
			}
			else {
				//LOGD("read size %d from packet", ret);
				info->pkt.data += ret;
				info->pkt.size -= ret;
			}

			if (info->pkt.size == 0) {
				if (info->free_orig_pkt){
					info->free_orig_pkt = false;
					av_free_packet(&info->orig_pkt);
				}
				//av_packet_unref(&info->orig_pkt);
			}
		}
		else {
			info->pkt.data = NULL;
			info->pkt.size = 0;
			ret = decode_packet(info, &got_frame);
			if (ret < 0) {
				//LOGE("can't decode packet flushed %s", info->src);
				return emptyArray;
			}
			if (got_frame == 0) {
				if (info->has_decoded_frames) {
					//LOGD("file end reached %s", info->src);
					if ((ret = avformat_seek_file(info->fmt_ctx, -1, std::numeric_limits<int64_t>::min(), 0, std::numeric_limits<int64_t>::max(), 0)) < 0) {
						//LOGE("can't seek to begin of file %s, %s", info->src, av_err2str(ret));
						return emptyArray;
					}
					else {
						avcodec_flush_buffers(info->video_dec_ctx);
					}
				}
			}
		}
		if (ret < 0) {
			return emptyArray;
		}

		if (got_frame) {

			auto prevStop = start;
			auto stop = (clock() - start);
			Log("elapsed1={0} {1}", (double)stop, (double)(stop - prevStop));

			//LOGD("decoded frame with w = %d, h = %d, format = %d", info->frame->width, info->frame->height, info->frame->format);
			auto pixelsLength = info->frame->width * info->frame->height * 4;
			auto pixels = ref new Platform::Array<uint8_t>(pixelsLength);

			if (info->frame->format == AV_PIX_FMT_YUV420P || info->frame->format == AV_PIX_FMT_BGRA) {
				//jint *dataArr = env->GetIntArrayElements(data, 0);
				if (data != nullptr && data->Length >= 3) {
					data[2] = (int)(1000 * info->frame->pkt_pts * av_q2d(info->video_stream->time_base));
					//env->ReleaseIntArrayElements(data, dataArr, 0);
				}

				if (info->frame->format == AV_PIX_FMT_YUV420P) {
					I420ToARGB(info->frame->data[0], info->frame->linesize[0], info->frame->data[2], info->frame->linesize[2], info->frame->data[1], info->frame->linesize[1], pixels->Data, info->frame->width * 4, info->frame->width, info->frame->height);
				}
				else if (info->frame->format == AV_PIX_FMT_BGRA) {
					ABGRToARGB(info->frame->data[0], info->frame->linesize[0], pixels->Data, info->frame->width * 4, info->frame->width, info->frame->height);
				}
			}
			prevStop = stop;
			stop = (clock() - start);
			Log("elapsed2={0} {1}", (double)stop, (double)(stop - prevStop));
			info->has_decoded_frames = true;
			av_frame_unref(info->frame);

			//prevStop = stop;
			//stop = (clock() - start);
			//Log("elapsed3={0} {1}", (double)stop, (double)(stop - prevStop));
			//auto returnBitmap = ref new Platform::Array<int>(pixelsLength / 4);
			//for (int j = 0; j < pixelsLength / 4; j++){
			//	returnBitmap[j] =
			//		(pixels[j * 4 + 3] << 24) +     //b
			//		(pixels[j * 4 + 2] << 0) +      //g
			//		(pixels[j * 4 + 1] << 8) +      //r
			//		(pixels[j * 4] << 16);          //a 
			//	//returnBitmap[i] = pixels[i];
			//}

			//delete[] pixels;

			prevStop = stop;
			stop = (clock() - start);// / (double)CLOCKS_PER_SEC;
			Log("elapsed4={0} {1}", (double)stop, (double)(stop - prevStop));
			return pixels;
		}
	}
	return emptyArray;
}