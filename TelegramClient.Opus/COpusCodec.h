//#include <stdexcept>
//#include <memory>
//
//struct OpusErrorException : public virtual std::exception
//{
//	OpusErrorException(int code) : code(code) {}
//	const char* what();
//private:
//	const int code;
//};
//
//struct COpusCodec
//{
//	COpusCodec(int32_t sampling_rate, int channels);
//	~COpusCodec();
//
//	bool decode_frame(std::ifstream& fin, std::ofstream& fout);
//	private:
//	struct Impl;
//	std::unique_ptr<Impl> _pimpl;
//};