// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#include "opus_defines.h"
#include "opus_types.h"


#include <ogg/ogg.h>
#include <stdio.h>
#include <opus.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include "opusfile.h"
#include "stdint.h"
#include "stdio.h"

#include "windows.h"


#include "pch.h"
#include "audio.c"
#include "TelegramClient.Opus.h"
#include <string>

using namespace TelegramClient_Opus;
using namespace Platform;
//using namespace Windows::Storage;

//---------------


typedef struct {
    int version;
    int channels; /* Number of channels: 1..255 */
    int preskip;
    ogg_uint32_t input_sample_rate;
    int gain; /* in dB S7.8 should be zero whenever possible */
    int channel_mapping;
    /* The rest is only used if channel_mapping != 0 */
    int nb_streams;
    int nb_coupled;
    unsigned char stream_map[255];
} OpusHeader;

typedef struct {
    unsigned char *data;
    int maxlen;
    int pos;
} Packet;

typedef struct {
    const unsigned char *data;
    int maxlen;
    int pos;
} ROPacket;

typedef struct {
    void *readdata;
    opus_int64 total_samples_per_channel;
    int rawmode;
    int channels;
    long rate;
    int gain;
    int samplesize;
    int endianness;
    char *infilename;
    int ignorelength;
    int skip;
    int extraout;
    char *comments;
    int comments_length;
    int copy_comments;
} oe_enc_opt;

static int write_uint32(Packet *p, ogg_uint32_t val) {
    if (p->pos > p->maxlen - 4) {
        return 0;
    }
    p->data[p->pos  ] = (val    ) & 0xFF;
    p->data[p->pos+1] = (val>> 8) & 0xFF;
    p->data[p->pos+2] = (val>>16) & 0xFF;
    p->data[p->pos+3] = (val>>24) & 0xFF;
    p->pos += 4;
    return 1;
}

static int write_uint16(Packet *p, ogg_uint16_t val) {
    if (p->pos > p->maxlen-2) {
        return 0;
    }
    p->data[p->pos  ] = (val    ) & 0xFF;
    p->data[p->pos+1] = (val>> 8) & 0xFF;
    p->pos += 2;
    return 1;
}

static int write_chars(Packet *p, const unsigned char *str, int nb_chars)
{
    int i;
    if (p->pos>p->maxlen-nb_chars)
        return 0;
    for (i=0;i<nb_chars;i++)
        p->data[p->pos++] = str[i];
    return 1;
}

static int read_uint32(ROPacket *p, ogg_uint32_t *val)
{
    if (p->pos>p->maxlen-4)
        return 0;
    *val =  (ogg_uint32_t)p->data[p->pos  ];
    *val |= (ogg_uint32_t)p->data[p->pos+1]<< 8;
    *val |= (ogg_uint32_t)p->data[p->pos+2]<<16;
    *val |= (ogg_uint32_t)p->data[p->pos+3]<<24;
    p->pos += 4;
    return 1;
}

static int read_uint16(ROPacket *p, ogg_uint16_t *val)
{
    if (p->pos>p->maxlen-2)
        return 0;
    *val =  (ogg_uint16_t)p->data[p->pos  ];
    *val |= (ogg_uint16_t)p->data[p->pos+1]<<8;
    p->pos += 2;
    return 1;
}

static int read_chars(ROPacket *p, unsigned char *str, int nb_chars)
{
    int i;
    if (p->pos>p->maxlen-nb_chars)
        return 0;
    for (i=0;i<nb_chars;i++)
        str[i] = p->data[p->pos++];
    return 1;
}

int opus_header_to_packet(const OpusHeader *h, unsigned char *packet, int len) {
    int i;
    Packet p;
    unsigned char ch;
    
    p.data = packet;
    p.maxlen = len;
    p.pos = 0;
    if (len < 19) {
        return 0;
    }
    if (!write_chars(&p, (const unsigned char *)"OpusHead", 8)) {
        return 0;
    }

    ch = 1;
    if (!write_chars(&p, &ch, 1)) {
        return 0;
    }
    
    ch = h->channels;
    if (!write_chars(&p, &ch, 1)) {
        return 0;
    }
    
    if (!write_uint16(&p, h->preskip)) {
        return 0;
    }
    
    if (!write_uint32(&p, h->input_sample_rate)) {
        return 0;
    }
    
    if (!write_uint16(&p, h->gain)) {
        return 0;
    }
    
    ch = h->channel_mapping;
    if (!write_chars(&p, &ch, 1)) {
        return 0;
    }
    
    if (h->channel_mapping != 0) {
        ch = h->nb_streams;
        if (!write_chars(&p, &ch, 1)) {
            return 0;
        }
        
        ch = h->nb_coupled;
        if (!write_chars(&p, &ch, 1)) {
            return 0;
        }
        
        /* Multi-stream support */
        for (i = 0; i < h->channels; i++) {
            if (!write_chars(&p, &h->stream_map[i], 1)) {
                return 0;
            }
        }
    }
    
    return p.pos;
}

#define writeint(buf, base, val) do { buf[base + 3] = ((val) >> 24) & 0xff; \
buf[base + 2]=((val) >> 16) & 0xff; \
buf[base + 1]=((val) >> 8) & 0xff; \
buf[base] = (val) & 0xff; \
} while(0)

static void comment_init(char **comments, int *length, const char *vendor_string) {
    // The 'vendor' field should be the actual encoding library used
    int vendor_length = strlen(vendor_string);
    int user_comment_list_length = 0;
    int len = 8 + 4 + vendor_length + 4;
    char *p = (char *)malloc(len);
    memcpy(p, "OpusTags", 8);
    writeint(p, 8, vendor_length);
    memcpy(p + 12, vendor_string, vendor_length);
    writeint(p, 12 + vendor_length, user_comment_list_length);
    *length = len;
    *comments = p;
}

static void comment_pad(char **comments, int* length, int amount) {
	char* p;
	int newlen;
	int i;
    if (amount > 0) {
        p = *comments;
        // Make sure there is at least amount worth of padding free, and round up to the maximum that fits in the current ogg segments
        newlen = (*length + amount + 255) / 255 * 255 - 1;
        p = (char*)realloc(p, newlen);
        for (i = *length; i < newlen; i++) {
            p[i] = 0;
        }
        *comments = p;
        *length = newlen;
    }
}

static int writeOggPage(ogg_page *page, FILE *os) {
    int written = fwrite(page->header, sizeof(unsigned char), page->header_len, os);
    written += fwrite(page->body, sizeof(unsigned char), page->body_len, os);
    return written;
}

const opus_int32 bitrate = 16000;
const opus_int32 rate = 16000;
const opus_int32 frame_size = 960;
const int with_cvbr = 1;
const int max_ogg_delay = 0;
const int comment_padding = 512;

opus_int32 coding_rate = 16000;
ogg_int32_t _packetId;
OpusEncoder *_encoder = 0;
uint8_t *_packet = 0;
ogg_stream_state os;
FILE *_fileOs = 0;
oe_enc_opt inopt;
OpusHeader header;
opus_int32 min_bytes;
int max_frame_bytes;
ogg_packet op;
ogg_page og;
opus_int64 bytes_written;
opus_int64 pages_out;
opus_int64 total_samples;
ogg_int64_t enc_granulepos;
ogg_int64_t last_granulepos;
int size_segments;
int last_segments;

void cleanupRecorder() {
    
    ogg_stream_flush(&os, &og);
    
    if (_encoder) {
        opus_encoder_destroy(_encoder);
        _encoder = 0;
    }
    
    ogg_stream_clear(&os);
    
    if (_packet) {
        free(_packet);
        _packet = 0;
    }
    
    if (_fileOs) {
        fclose(_fileOs);
        _fileOs = 0;
    }
    
    _packetId = -1;
    bytes_written = 0;
    pages_out = 0;
    total_samples = 0;
    enc_granulepos = 0;
    size_segments = 0;
    last_segments = 0;
    last_granulepos = 0;
    memset(&os, 0, sizeof(ogg_stream_state));
    memset(&inopt, 0, sizeof(oe_enc_opt));
    memset(&header, 0, sizeof(OpusHeader));
    memset(&op, 0, sizeof(ogg_packet));
    memset(&og, 0, sizeof(ogg_page));
}

int initRecorder(const char *path) {
    cleanupRecorder();
    
    if (!path) {
        return 0;
    }
    
    _fileOs = fopen(path, "wb");
    if (!_fileOs) {
        return 0;
    }
    
    inopt.rate = rate;
    inopt.gain = 0;
    inopt.endianness = 0;
    inopt.copy_comments = 0;
    inopt.rawmode = 1;
    inopt.ignorelength = 1;
    inopt.samplesize = 16;
    inopt.channels = 1;
    inopt.skip = 0;
    
    comment_init(&inopt.comments, &inopt.comments_length, opus_get_version_string());
    
    if (rate > 24000) {
        coding_rate = 48000;
    } else if (rate > 16000) {
        coding_rate = 24000;
    } else if (rate > 12000) {
        coding_rate = 16000;
    } else if (rate > 8000) {
        coding_rate = 12000;
    } else {
        coding_rate = 8000;
    }
    
    if (rate != coding_rate) {
        //LOGE("Invalid rate");
        return 0;
    }
    
    header.channels = 1;
    header.channel_mapping = 0;
    header.input_sample_rate = rate;
    header.gain = inopt.gain;
    header.nb_streams = 1;
    
    int result = OPUS_OK;
    _encoder = opus_encoder_create(coding_rate, 1, OPUS_APPLICATION_AUDIO, &result);
    if (result != OPUS_OK) {
        //LOGE("Error cannot create encoder: %s", opus_strerror(result));
        return 0;
    }
    
    min_bytes = max_frame_bytes = (1275 * 3 + 7) * header.nb_streams;
	_packet = (uint8_t*)malloc(max_frame_bytes);
    
    result = opus_encoder_ctl(_encoder, OPUS_SET_BITRATE(bitrate));
    if (result != OPUS_OK) {
        //LOGE("Error OPUS_SET_BITRATE returned: %s", opus_strerror(result));
        return 0;
    }
    
#ifdef OPUS_SET_LSB_DEPTH
    result = opus_encoder_ctl(_encoder, OPUS_SET_LSB_DEPTH(max(8, min(24, inopt.samplesize))));
    if (result != OPUS_OK) {
        //LOGE("Warning OPUS_SET_LSB_DEPTH returned: %s", opus_strerror(result));
    }
#endif
    
    opus_int32 lookahead;
    result = opus_encoder_ctl(_encoder, OPUS_GET_LOOKAHEAD(&lookahead));
    if (result != OPUS_OK) {
        //LOGE("Error OPUS_GET_LOOKAHEAD returned: %s", opus_strerror(result));
        return 0;
    }
    
    inopt.skip += lookahead;
    header.preskip = (int)(inopt.skip * (48000.0 / coding_rate));
    inopt.extraout = (int)(header.preskip * (rate / 48000.0));
    
    if (ogg_stream_init(&os, rand()) == -1) {
        //LOGE("Error: stream init failed");
        return 0;
    }
    
    unsigned char header_data[100];
    int packet_size = opus_header_to_packet(&header, header_data, 100);
    op.packet = header_data;
    op.bytes = packet_size;
    op.b_o_s = 1;
    op.e_o_s = 0;
    op.granulepos = 0;
    op.packetno = 0;
    ogg_stream_packetin(&os, &op);
    
    while ((result = ogg_stream_flush(&os, &og))) {
        if (!result) {
            break;
        }
        
        int pageBytesWritten = writeOggPage(&og, _fileOs);
        if (pageBytesWritten != og.header_len + og.body_len) {
            //LOGE("Error: failed writing header to output stream");
            return 0;
        }
        bytes_written += pageBytesWritten;
        pages_out++;
    }
    
    comment_pad(&inopt.comments, &inopt.comments_length, comment_padding);
    op.packet = (unsigned char *)inopt.comments;
    op.bytes = inopt.comments_length;
    op.b_o_s = 0;
    op.e_o_s = 0;
    op.granulepos = 0;
    op.packetno = 1;
    ogg_stream_packetin(&os, &op);
    
    while ((result = ogg_stream_flush(&os, &og))) {
        if (result == 0) {
            break;
        }
        
        int writtenPageBytes = writeOggPage(&og, _fileOs);
        if (writtenPageBytes != og.header_len + og.body_len) {
            //LOGE("Error: failed writing header to output stream");
            return 0;
        }
        
        bytes_written += writtenPageBytes;
        pages_out++;
    }
    
    free(inopt.comments);
    
    return 1;
}

int writeFrame(uint8_t *framePcmBytes, unsigned int frameByteCount) {
    int cur_frame_size = frame_size;
    _packetId++;
    
    opus_int32 nb_samples = frameByteCount / 2;
    total_samples += nb_samples;
    if (nb_samples < frame_size) {
        op.e_o_s = 1;
    } else {
        op.e_o_s = 0;
    }
    
    int nbBytes = 0;
    
    if (nb_samples != 0) {
        uint8_t *paddedFrameBytes = framePcmBytes;
        int freePaddedFrameBytes = 0;
        
        if (nb_samples < cur_frame_size) {
            paddedFrameBytes = (uint8_t*)malloc(cur_frame_size * 2);
            freePaddedFrameBytes = 1;
            memcpy(paddedFrameBytes, framePcmBytes, frameByteCount);
            memset(paddedFrameBytes + nb_samples * 2, 0, cur_frame_size * 2 - nb_samples * 2);
        }
        
        nbBytes = opus_encode(_encoder, (opus_int16 *)paddedFrameBytes, cur_frame_size, _packet, max_frame_bytes / 10);
        if (freePaddedFrameBytes) {
            free(paddedFrameBytes);
            paddedFrameBytes = NULL;
        }
        
        if (nbBytes < 0) {
            //LOGE("Encoding failed: %s. Aborting.", opus_strerror(nbBytes));
            return 0;
        }
        
        enc_granulepos += cur_frame_size * 48000 / coding_rate;
        size_segments = (nbBytes + 255) / 255;
        min_bytes = min(nbBytes, min_bytes);
    }
    
    while ((((size_segments <= 255) && (last_segments + size_segments > 255)) || (enc_granulepos - last_granulepos > max_ogg_delay)) && ogg_stream_flush_fill(&os, &og, 255 * 255)) {
        if (ogg_page_packets(&og) != 0) {
            last_granulepos = ogg_page_granulepos(&og);
        }
        
        last_segments -= og.header[26];
        int writtenPageBytes = writeOggPage(&og, _fileOs);
        if (writtenPageBytes != og.header_len + og.body_len) {
            //LOGE("Error: failed writing data to output stream");
            return 0;
        }
        bytes_written += writtenPageBytes;
        pages_out++;
    }
    
    op.packet = (unsigned char *)_packet;
    op.bytes = nbBytes;
    op.b_o_s = 0;
    op.granulepos = enc_granulepos;
    if (op.e_o_s) {
        op.granulepos = ((total_samples * 48000 + rate - 1) / rate) + header.preskip;
    }
    op.packetno = 2 + _packetId;
    ogg_stream_packetin(&os, &op);
    last_segments += size_segments;
    
    while ((op.e_o_s || (enc_granulepos + (frame_size * 48000 / coding_rate) - last_granulepos > max_ogg_delay) || (last_segments >= 255)) ? ogg_stream_flush_fill(&os, &og, 255 * 255) : ogg_stream_pageout_fill(&os, &og, 255 * 255)) {
        if (ogg_page_packets(&og) != 0) {
            last_granulepos = ogg_page_granulepos(&og);
        }
        last_segments -= og.header[26];
        int writtenPageBytes = writeOggPage(&og, _fileOs);
        if (writtenPageBytes != og.header_len + og.body_len) {
            //LOGE("Error: failed writing data to output stream");
            return 0;
        }
        bytes_written += writtenPageBytes;
        pages_out++;
    }
    
    return 1;
}

//player
OggOpusFile *_opusFile;
int _isSeekable = 0;
long _totalPcmDuration = 0;
long _currentPcmOffset = 0;
int _finished = 0;
static const int playerBuffersCount = 3;
static const int playerSampleRate = 48000;

void cleanupPlayer() {
    if (_opusFile) {
        op_free(_opusFile);
        _opusFile = 0;
    }
    _isSeekable = 0;
    _totalPcmDuration = 0;
    _currentPcmOffset = 0;
    _finished = 0;
}

int seekPlayer(float position) {
    int result;
	ogg_int64_t pcmPosition;

	if (!_opusFile || !_isSeekable || position < 0) {
        return 0;
    }
    result = op_pcm_seek(_opusFile, (ogg_int64_t)(position * _totalPcmDuration));
    if (result != OPUS_OK) {
        //LOGE("op_pcm_seek failed: %d", result);
    }
    pcmPosition = op_pcm_tell(_opusFile);
    _currentPcmOffset = pcmPosition;
    return result == OPUS_OK;
}

int initPlayer(const char *path) {
    int openError = OPUS_OK;
    cleanupPlayer();
    
    _opusFile = op_open_file(path, &openError);
    if (!_opusFile || openError != OPUS_OK) {
        //LOGE("op_open_file failed: %d", openError);
        cleanupPlayer();
        return 0;
    }
    
    _isSeekable = op_seekable(_opusFile);
    _totalPcmDuration = op_pcm_total(_opusFile, -1);
        
    return 1;
}

void fillBuffer(uint8_t *buffer, int capacity, int *args) {
    if (_opusFile) {
		args[1] = max((ogg_int64_t)0, op_pcm_tell(_opusFile));
        
        if (_finished) {
            args[0] = 0;
            args[1] = 0;
            args[2] = 1;
            return;
        } else {
            int writtenOutputBytes = 0;
            int endOfFileReached = 0;
            
            while (writtenOutputBytes < capacity) {
                int readSamples = op_read(_opusFile, (opus_int16 *)(buffer + writtenOutputBytes), (capacity - writtenOutputBytes) / 2, NULL);
                
                if (readSamples > 0) {
                    writtenOutputBytes += readSamples * 2;
                } else {
                    if (readSamples < 0) {
                        //LOGE("op_read failed: %d", readSamples);
                    }
                    endOfFileReached = 1;
                    break;
                }
            }
            
            args[0] = writtenOutputBytes;
            
            if (endOfFileReached || args[1] + args[0] == _totalPcmDuration) {
                _finished = 1;
                args[2] = 1;
            } else {
                args[2] = 0;
            }
        }
    } else {
        memset(buffer, 0, capacity);
        args[0] = capacity;
        args[1] = _totalPcmDuration;
    }
}

//--------------------------------------------

int startRecord(char* pathStr) {  
    int result = initRecorder(pathStr);
    
    return result;
}

void stopRecord() {
    cleanupRecorder();
}

long getTotalPcmDuration() {
    return _totalPcmDuration;
}

void readOpusFile(uint8_t* bufferBytes, int capacity, int* args) {
    fillBuffer(bufferBytes, capacity, args);
}

int seekOpusFile(float position) {
    return seekPlayer(position);
}

int openOpusFile(char* pathStr) {
    
    int result = initPlayer(pathStr);
    
    return result;
}

void closeOpusFile() {
    cleanupPlayer();
}

int isOpusFile(char* pathStr) {    
    int result = 0;
    
    int error = OPUS_OK;
    OggOpusFile *file = op_test_file(pathStr, &error);
    if (file != NULL) {
        int error = op_test_open(file);
        op_free(file);
        
        result = error == OPUS_OK;
    }
    
    return result;
}
//---------------

WindowsPhoneRuntimeComponent::WindowsPhoneRuntimeComponent()
{

}

const char* StringToCharArray(Platform::String^ str){
	std::wstring fooW(str->Begin());
	std::string fooA(fooW.begin(), fooW.end());
	return fooA.c_str();
}

int WindowsPhoneRuntimeComponent::Sum(int a, int b)
{
	auto local = Windows::Storage::ApplicationData::Current->LocalFolder;
	auto localFileNamePlatformString = local->Path + "\\game.sav";
	return a + b;
}

int64 WindowsPhoneRuntimeComponent::GetTotalPcmDuration(){
	return getTotalPcmDuration();
}

int WindowsPhoneRuntimeComponent::InitPlayer(Platform::String^ path){
	std::wstring fooW(path->Begin());
	std::string fooA(fooW.begin(), fooW.end());
	const char* pathStr = fooA.c_str();
	int result = initPlayer(pathStr);
	//delete[] pathStr;

	return result;
}
		
void WindowsPhoneRuntimeComponent::CleanupPlayer(){
	cleanupPlayer();
}

void WindowsPhoneRuntimeComponent::FillBuffer(Platform::WriteOnlyArray<uint8>^ buffer, int capacity, Platform::WriteOnlyArray<int>^ args){
	uint8_t* buf = new uint8_t[capacity]; 
	int* bufArgs = new int[3];

	fillBuffer(buf, capacity, bufArgs);

	for (int i = 0; i < capacity; i++){
		buffer->set(i, buf[i]);
	}

	args->set(0, bufArgs[0]);
	args->set(1, bufArgs[1]);
	args->set(2, bufArgs[2]);

	delete[] buf;
	delete[] bufArgs;
}

bool WindowsPhoneRuntimeComponent::IsOpusFile(Platform::String^ path) {
    Platform::String^ fooRT = path;
	std::wstring fooW(fooRT->Begin());
	std::string fooA(fooW.begin(), fooW.end());
	const char* charStr = fooA.c_str();
	
	//const char *pathStr = (*env)->GetStringUTFChars(env, path, 0);
    
    int error = OPUS_OK;
	bool result = false;
    OggOpusFile *file = op_test_file(charStr, &error);
    if (file != NULL) {
        int error = op_test_open(file);
        op_free(file);
        
        result = error == OPUS_OK;
    }

    
    /*if (pathStr != 0) {
        (*env)->ReleaseStringUTFChars(env, path, pathStr);
    }*/
    
    return result;
}

int WindowsPhoneRuntimeComponent::StartRecord(Platform::String^ path) {
	Platform::String^ fooRT = path;
	std::wstring fooW(fooRT->Begin());
	std::string fooA(fooW.begin(), fooW.end());
	const char* charStr = fooA.c_str();

    int result = initRecorder(charStr);
    
    /*if (pathStr != 0) {
        (*env)->ReleaseStringUTFChars(env, path, pathStr);
    }*/
    
    return result;
}
		
int WindowsPhoneRuntimeComponent::WriteFrame(const Platform::Array<uint8>^ buffer, int length) {
	return writeFrame(buffer->begin(), length);
}
		
void WindowsPhoneRuntimeComponent::StopRecord() {
	cleanupRecorder();
}

static inline void set_bits(uint8_t *bytes, int32_t bitOffset, int32_t numBits, int32_t value) {
	numBits = (unsigned int)(2 << (numBits - 1)) - 1;
	bytes += bitOffset / 8;
	bitOffset %= 8;
	*((int32_t *)bytes) |= (value << bitOffset);
}

Platform::Array<uint8>^ WindowsPhoneRuntimeComponent::GetWaveform(Platform::String^ path) {
	Platform::String^ fooRT = path;
	std::wstring fooW(fooRT->Begin());
	std::string fooA(fooW.begin(), fooW.end());
	const char* pathStr = fooA.c_str();

	Platform::Array<uint8_t>^ result = ref new Platform::Array<uint8_t>(0);// ref new Platform::Array<unsigned char>(retData, retDataLength);
	//jbyteArray result = 0;

	int error = OPUS_OK;
	OggOpusFile *opusFile = op_open_file(pathStr, &error);
	if (opusFile != NULL && error == OPUS_OK) {
		int64_t totalSamples = op_pcm_total(opusFile, -1);
		int32_t resultSamples = 100;
		int32_t sampleRate = (int32_t)max(1, totalSamples / resultSamples);

		uint16_t *samples = new uint16_t[100];

		int bufferSize = 1024 * 128;
		int16_t *sampleBuffer = new int16_t[bufferSize];
		uint64_t sampleIndex = 0;
		uint16_t peakSample = 0;

		int index = 0;

		while (1) {
			int readSamples = op_read(opusFile, sampleBuffer, bufferSize / 2, NULL);
			for (int i = 0; i < readSamples; i++) {
				uint16_t sample = (uint16_t) abs(sampleBuffer[i]);
				if (sample > peakSample) {
					peakSample = sample;
				}
				if (sampleIndex++ % sampleRate == 0) {
					if (index < resultSamples) {
						samples[index++] = peakSample;
					}
					peakSample = 0;
				}
			}
			if (readSamples == 0) {
				break;
			}
		}

		/*uint16_t peak = 0;
		for (int i = 0; i < resultSamples; i++) {
			if (peak < samples[i]){
				peak = samples[i];
			}
		}

		delete[] sampleBuffer;
		op_free(opusFile);

		uint32_t bitstreamLength = (resultSamples * 5) / 8 + (((resultSamples * 5) % 8) == 0 ? 0 : 1);

		result = ref new Platform::Array<unsigned char>(resultSamples);

		uint8_t* bytes = new uint8_t[bitstreamLength];

		for (int i = 0; i < resultSamples; i++) {
			int32_t value = min(31, abs((int32_t)samples[i]) * 31 / peak);

			result[i] = value & 31;
			set_bits(bytes, i * 5, 5, value & 31);
		}

		delete[] bytes;
		delete[] samples;*/

		int64_t sumSamples = 0;
		for (int i = 0; i < resultSamples; i++) {
			sumSamples += samples[i];
		}
		uint16_t peak = (uint16_t) (sumSamples * 1.8f / resultSamples);
		if (peak < 7500) {
			peak = 7500;
		}

		for (int i = 0; i < resultSamples; i++) {
			uint16_t sample = samples[i];//(uint16_t) ((int64_t) samples[i]);
			if (sample > peak) {
				samples[i] = peak;
			}
		}

		delete[] sampleBuffer;
		op_free(opusFile);

		uint32_t bitstreamLength = (resultSamples * 5) / 8 + (((resultSamples * 5) % 8) == 0 ? 0 : 1);

		result = ref new Platform::Array<unsigned char>(resultSamples);
		//result = ref new Platform::Array<unsigned char>(bitstreamLength);
		
		uint8_t* bytes = new uint8_t[bitstreamLength];

		for (int i = 0; i < resultSamples; i++) {
			int32_t value = min(31, abs((int32_t)samples[i]) * 31 / peak);
			
			result[i] = value & 31;
			set_bits(bytes, i * 5, 5, value & 31);
		}
		
		
		/*for (int i = 0; i < bitstreamLength; i++) {
			result[i] = bytes[i];
		}*/


		delete[] bytes;
		delete[] samples;
	}

	return result;
}




//void WindowsPhoneRuntimeComponent::WriteFile( String^ strFile, String^ strContent )
//{
//    auto folder = ApplicationData::Current->LocalFolder;
    //task<StorageFile^> getFileTask(folder->CreateFileAsync( strFile, CreationCollisionOption::ReplaceExisting));

    //// Create a local to allow the DataReader to be passed between lambdas.
    //auto writer = std::make_shared<Streams::DataWriter^>(nullptr);

    //getFileTask.then([](StorageFile^ file)
    //{
    //    return file->OpenAsync(FileAccessMode::ReadWrite);
    //}).then([this, writer, strContent](Streams::IRandomAccessStream^ stream)
    //{
    //    Streams::DataWriter^ state = ref new Streams::DataWriter(stream);
    //    *writer = state;

    //    unsigned int codeUnits = state->MeasureString(strContent);
    //    state->WriteUInt32(codeUnits);
    //    state->WriteString(strContent);

    //    return state->StoreAsync();
    //}).then([writer](uint32 count)
    //{
    //    return (*writer)->FlushAsync();
    //}).then([this, writer](bool flushed)
    //{
    //    delete (*writer);
    //});
//}

//void WindowsPhoneRuntimeComponent::LoadFile(String^ strFile)
//{
 //   auto folder = ApplicationData::Current->LocalFolder;
    //task<StorageFile^> getFileTask(folder->GetFileAsync(strFile));

    //// Create a local to allow the DataReader to be passed between lambdas.
    //auto reader = std::make_shared<Streams::DataReader^>(nullptr);
    //getFileTask.then([this, reader](task<StorageFile^> fileTask)
    //{
    //    try
    //    {
    //        StorageFile^ file = fileTask.get();

    //        task<Streams::IRandomAccessStreamWithContentType^> (file->OpenReadAsync()).then([reader](Streams::IRandomAccessStreamWithContentType^ stream)
    //        {
    //            *reader = ref new Streams::DataReader(stream);
    //            return (*reader)->LoadAsync(static_cast<uint32>(stream->Size));
    //        }).then([this, reader](uint32 bytesRead)
    //        {

    //            Streams::DataReader^ state = (*reader);
    //            try
    //            {
    //                    unsigned int codeUnits = state->ReadUInt32();
    //                    Platform::String^ strContent = state->ReadString(codeUnits);

    //            }
    //            catch (Platform::Exception^ e)
    //            {
    //                // Do nothing.
    //            }
    //        });;
    //    }
    //    catch (Platform::Exception^ e)
    //    {

    //    }
    //});
//}

