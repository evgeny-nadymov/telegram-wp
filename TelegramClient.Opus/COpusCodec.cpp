//// (c) Seth Heeren 2013
////
//// Based on src/opus_demo.c in opus-1.0.2
//// License see http://www.opus-codec.org/license/
//#include "COpusCodec.h"
//#include <vector>
//#include <iomanip>
//#include <memory>
//#include <sstream>
//#include <fstream>
//
//#include "opus.h"
//
//#define MAX_PACKET 1500
//
//const char* OpusErrorException::what()
//{
//    return opus_strerror(code);
//}
//
//// I'd suggest reading with boost::spirit::big_dword or similar
//static uint32_t char_to_int(char ch[4])
//{
//    return static_cast<uint32_t>(static_cast<unsigned char>(ch[0])<<24) |
//           static_cast<uint32_t>(static_cast<unsigned char>(ch[1])<<16) |
//           static_cast<uint32_t>(static_cast<unsigned char>(ch[2])<< 8) |
//           static_cast<uint32_t>(static_cast<unsigned char>(ch[3])<< 0);
//}
//
//struct COpusCodec::Impl
//{
//    Impl(int32_t sampling_rate = 48000, int channels = 1)
//      : 
//          _channels(channels),
//          _decoder(nullptr, &opus_decoder_destroy),
//          _state(_max_frame_size, MAX_PACKET, channels)
//    {
//        int err = OPUS_OK;
//        auto raw = opus_decoder_create(sampling_rate, _channels, &err);
//        _decoder.reset(err == OPUS_OK? raw : throw OpusErrorException(err) );
//    }
//
//    bool decode_frame(std::ifstream& fin, std::ofstream& fout)
//    {
//        char ch[4] = {0};
//
//        if (!fin.read(ch, 4) && fin.eof())
//            return false;
//
//        uint32_t len = char_to_int(ch);
//
//        if(len>_state.data.size())
//            throw std::runtime_error("Invalid payload length");
//
//        fin.read(ch, 4);
//        const uint32_t enc_final_range = char_to_int(ch);
//        const auto data = reinterpret_cast<char*>(&_state.data.front());
//
//        size_t read = 0ul;
//        for (auto append_position = data; fin && read<len; append_position += read)
//        {
//            read += fin.readsome(append_position, len-read);
//        }
//
//        if(read<len)
//        {
//            std::ostringstream oss;
//            oss << "Ran out of input, expecting " << len << " bytes got " << read << " at " << fin.tellg();
//            throw std::runtime_error(oss.str());
//        }
//
//        int output_samples;
//        const bool lost = (len==0);
//        if(lost)
//        {
//            opus_decoder_ctl(_decoder.get(), OPUS_GET_LAST_PACKET_DURATION(&output_samples));
//        }
//        else
//        {
//            output_samples = _max_frame_size;
//        }
//
//        output_samples = opus_decode(
//                _decoder.get(), 
//                lost ? NULL : _state.data.data(),
//                len,
//                _state.out.data(),
//                output_samples,
//                0);
//
//        if(output_samples>0)
//        {
//            for(int i=0; i<(output_samples)*_channels; i++)
//            {
//                short s;
//                s=_state.out[i];
//                _state.fbytes[2*i]   = s&0xFF;
//                _state.fbytes[2*i+1] = (s>>8)&0xFF;
//            }
//            if(!fout.write(reinterpret_cast<char*>(_state.fbytes.data()), sizeof(short)* _channels * output_samples))
//                throw std::runtime_error("Error writing");
//        }
//        else
//        {
//            throw OpusErrorException(output_samples); // negative return is error code
//        }
//
//        uint32_t dec_final_range;
//        opus_decoder_ctl(_decoder.get(), OPUS_GET_FINAL_RANGE(&dec_final_range));
//
//        /* compare final range encoder rng values of encoder and decoder */
//        if(enc_final_range!=0
//                && !lost && !_state.lost_prev
//                && dec_final_range != enc_final_range)
//        {
//            std::ostringstream oss;
//            oss << "Error: Range coder state mismatch between encoder and decoder in frame " << _state.frameno << ": " <<
//                    "0x" << std::setw(8) << std::setfill('0') << std::hex << (unsigned long)enc_final_range <<
//                    "0x" << std::setw(8) << std::setfill('0') << std::hex << (unsigned long)dec_final_range;
//
//            throw std::runtime_error(oss.str());
//        }
//
//        _state.lost_prev = lost;
//        _state.frameno++;
//
//        return true;
//    }
//private:
//    const int _channels;
//    const int _max_frame_size = 5760;// 960*6;
//    std::unique_ptr<OpusDecoder, void(*)(OpusDecoder*)> _decoder;
//
//    struct State
//    {
//        State(int max_frame_size, int max_payload_bytes, int channels) :
//            out   (max_frame_size*channels),
//            fbytes(max_frame_size*channels*sizeof(decltype(out)::value_type)),
//            data  (max_payload_bytes)
//        { }
//
//        std::vector<short>         out;
//        std::vector<unsigned char> fbytes, data;
//        int32_t frameno   = 0;
//        bool    lost_prev = true;
//    };
//    State _state;
//};
//
//COpusCodec::COpusCodec(int32_t sampling_rate, int channels)
//    : _pimpl(std::unique_ptr<Impl>(new Impl(sampling_rate, channels)))
//{
//    //
//}
//
//COpusCodec::~COpusCodec()
//{
//    // this instantiates the pimpl deletor code on the, now-complete, pimpl class
//}
//
//bool COpusCodec::decode_frame(
//        std::ifstream& fin,
//        std::ofstream& fout)
//{
//    return _pimpl->decode_frame(fin, fout);
//}