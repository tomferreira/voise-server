Convert from s16le to flac
-----------------------------------

ffmpeg.exe -i <input wav> -ar 16k -acodec flac <output flac>