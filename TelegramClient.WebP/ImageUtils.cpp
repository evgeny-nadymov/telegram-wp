// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#include "pch.h"
#include "ImageUtils.h"
#include <time.h>
#include <limits.h>
#include <string.h>
#include <stdio.h>
#include <stdint.h>
#include <stdlib.h>
#include <math.h>
#include <assert.h>

using namespace TelegramClient_WebP;

ImageUtils::ImageUtils()
{
}

#define SIZE_FULL 8100

static inline uint64_t get_colors(const uint8_t *p) {
	return p[0] + (p[1] << 16) + ((uint64_t)p[2] << 32);
}

static void fastBlur(int width, int height, int s, void *pixels, int radius) {
	uint8_t *pix = (uint8_t *)pixels;
	const int w = width;
	const int h = height;
	const int stride = s;
	const int r1 = radius + 1;
	const int div = radius * 2 + 1;
	int shift;
	if (radius == 1) {
		shift = 2;
	}
	else if (radius == 3) {
		shift = 4;
	}
	else if (radius == 7) {
		shift = 6;
	}
	else if (radius == 15) {
		shift = 8;
	}
	else {
		return;
	}

	if (radius > 15 || div >= w || div >= h || w * h > SIZE_FULL) {
		return;
	}

	static uint64_t rgb[SIZE_FULL];

	int x, y, i;

	int yw = 0;
	const int we = w - r1;
	for (y = 0; y < h; y++) {
		uint64_t cur = get_colors(&pix[yw]);
		uint64_t rgballsum = -radius * cur;
		uint64_t rgbsum = cur * ((r1 * (r1 + 1)) >> 1);

		for (i = 1; i <= radius; i++) {
			uint64_t cur = get_colors(&pix[yw + i * 4]);
			rgbsum += cur * (r1 - i);
			rgballsum += cur;
		}

		x = 0;

#define update(start, middle, end)                         \
      rgb[y * w + x] = (rgbsum >> shift) & 0x00FF00FF00FF00FF; \
                                                           \
      rgballsum += get_colors (&pix[yw + (start) * 4]) -   \
               2 * get_colors (&pix[yw + (middle) * 4]) +  \
                   get_colors (&pix[yw + (end) * 4]);      \
      rgbsum += rgballsum;                                 \
      x++;                                                 \

		while (x < r1) {
			update(0, x, x + r1);
		}
		while (x < we) {
			update(x - r1, x, x + r1);
		}
		while (x < w) {
			update(x - r1, x, w - 1);
		}
#undef update

		yw += stride;
	}

	const int he = h - r1;
	for (x = 0; x < w; x++) {
		uint64_t rgballsum = -radius * rgb[x];
		uint64_t rgbsum = rgb[x] * ((r1 * (r1 + 1)) >> 1);
		for (i = 1; i <= radius; i++) {
			rgbsum += rgb[i * w + x] * (r1 - i);
			rgballsum += rgb[i * w + x];
		}

		y = 0;
		int yi = x * 4;

#define update(start, middle, end)         \
      int64_t res = rgbsum >> shift;           \
      pix[yi] = res;                       \
      pix[yi + 1] = res >> 16;             \
      pix[yi + 2] = res >> 32;             \
                                           \
      rgballsum += rgb[x + (start) * w] -  \
               2 * rgb[x + (middle) * w] + \
                   rgb[x + (end) * w];     \
      rgbsum += rgballsum;                 \
      y++;                                 \
      yi += stride;

		while (y < r1) {
			update(0, y, y + r1);
		}
		while (y < he) {
			update(y - r1, y, y + r1);
		}
		while (y < h) {
			update(y - r1, y, h - 1);
		}
#undef update
	}
}

Platform::Array<uint8>^ ImageUtils::FastBlur(int width, int height, int stride, const Platform::Array<uint8>^ pixels){

	auto pix = new uint8_t[pixels->Length];
	for (int i = 0; i < pixels->Length; i++) {
		pix[i] = pixels[i];
	}

	fastBlur(width, height, stride, pix, 3);

	Platform::Array<uint8>^ returnPixels = ref new Platform::Array<uint8>(pixels->Length);
	for (int i = 0; i < pixels->Length; i++) {
		returnPixels[i] = pix[i];
	}

	delete[] pix;

	return returnPixels;
}

Platform::Array<uint8>^ ImageUtils::FastSecretBlur(int width, int height, int stride, const Platform::Array<uint8>^ pixels){

	auto pix = new uint8_t[pixels->Length];
	for (int i = 0; i < pixels->Length; i++) {
		pix[i] = pixels[i];
	}

	fastBlur(width, height, stride, pix, 15);

	Platform::Array<uint8>^ returnPixels = ref new Platform::Array<uint8>(pixels->Length);
	for (int i = 0; i < pixels->Length; i++) {
		returnPixels[i] = pix[i];
	}

	delete[] pix;

	return returnPixels;
}
