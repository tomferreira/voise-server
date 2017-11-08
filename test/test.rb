require 'voise'

class VoiseClientTest

  VOISE_HOST = '10.100.5.65'
  VOISE_PORT = 8102

  # Buffer size in bytes
  BYTES_PER_BUFFER = 3200

  # Bytes per sample for LINEAR16 or FLAC
  BYTES_PER_SAMPLE = 2

  def initialize
    @client = Voise::Client.new(VOISE_HOST, VOISE_PORT)
  end

  # client = VoiseClientTest.new
  # client.recognize("FLAC", 16000, "pt-BR", nil, "felicitacao/localizacao", "./alo.flac")
  # client.recognize("LINEAR16", 8000, "pt-BR", nil, "felicitacao/localizacao", "./Alo {duvida}.wav")
  # client.recognize("LINEAR16", 8000, "pt-BR", nil, "felicitacao/localizacao", "./Sem voz {#NOVOICE}.wav")
  def recognize(encoding, sample_rate, language_code, context, model_name, audio_file, engine = nil)
    time_method do
      puts @client.recognize(encoding, sample_rate, language_code, context, model_name, audio_file, engine).to_pretty_s
    end
  end

  # VoiseClientTest.test1("me", 2)
  def self.test1(engine, max_sleep_sec)
    threads = []
    100.times do |i| 
      threads << Thread.new do
        client = VoiseClientTest.new
        client.stream_recognize("LINEAR16", 8000, "pt-BR", nil, "felicitacao/localizacao", "./Alo {duvida}.wav", engine)
        client.close
        puts "Thread #{i+1} finnished"
      end

      # Sleep until 'max_sleep_sec' seconds
      sleep(rand * max_sleep_sec)
    end
    threads.each {|t| t.join }

    puts "-------------- FIM --------------"
  end

  # VoiseClientTest.test2("me")
  def self.test2(engine)
    client = VoiseClientTest.new
    100.times do |i| 
      client.stream_recognize("LINEAR16", 8000, "pt-BR", nil, "felicitacao/localizacao", "./Alo {duvida}.wav", engine)
    end
    client.close
  end

  def self.test3
    client = VoiseClientTest.new
    100.times do |i| 
      client.say("Mensagem de teste", "alaw", 8000, "pt-BR", "result.wav")
    end
    client.close
  end

  # client = VoiseClientTest.new
  # client.stream_recognize("FLAC", 16000, "pt-BR", nil, "felicitacao/localizacao", "./alo.flac")
  # client.stream_recognize("LINEAR16", 8000, "pt-BR", nil, "felicitacao/localizacao", "./Alo {duvida}.wav")
  def stream_recognize(encoding, sample_rate, language_code, context, model_name, audio_file, engine = nil)

    response = @client.start_streaming_recognize(encoding, sample_rate, language_code, context, model_name, engine)

    # If the start was not accepted..
    if response.result.code != 201
      puts response.to_pretty_s
      return
    end

    total_bytes = 0

    samples_per_buffer = BYTES_PER_BUFFER / BYTES_PER_SAMPLE
    samples_per_millis = sample_rate / 1000
    
    while (buffer = IO.binread(audio_file, BYTES_PER_BUFFER, total_bytes)) != nil do 
      total_bytes += buffer.length

      @client.data_streaming_recognize(buffer)

      # To simulate real-time audio, sleep after sending each audio buffer
      sleep(samples_per_buffer / samples_per_millis / 1000.0)
    end

    time_method do
      reponse = @client.stop_streaming_recognize
      puts reponse.to_pretty_s
    end
  end

  # client = VoiseClientTest.new
  # client.say("Mensagem de teste", "alaw", 8000, "pt-BR", "result.wav")
  def say(text, encoding, sample_rate, language_code, filename)
    response, audio_content = @client.say(text, encoding, sample_rate, language_code)

    if response.result.code == 200
      IO.binwrite(filename, audio_content)
    else
      puts response.to_pretty_s
    end
  end

  def close
    @client.close
  end

private

  def time_method(method = nil, *args)
    beginning_time = Time.now

    if block_given?
      yield
    else
      self.send(method, args)
    end

    end_time = Time.now

    puts "Time elapsed #{(end_time - beginning_time) * 1000} milliseconds"
  end
end