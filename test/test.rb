require 'voise'

class VoiseClientTest

  VOISE_HOST = '127.0.0.1'
  VOISE_PORT = 8102

  # Buffer size in bytes
  BYTES_PER_BUFFER = 320

  # Bytes per sample for LINEAR16 or FLAC
  BYTES_PER_SAMPLE = 2

  def initialize
    @client = Voise::Client.new(VOISE_HOST, VOISE_PORT)
  end

  # VoiseClientTest.concurrence_stream_test("LINEAR16", 8000, "pt-BR", nil, "felicitacao/localizacao", "./Alo {duvida}.wav", "me", 2)
  # VoiseClientTest.concurrence_stream_test("LINEAR16", 8000, "pt-BR", nil, "felicitacao/localizacao", "./Sem voz {#NOVOICE}.wav", "me", 1)
  def self.concurrence_stream_test(encoding, sample_rate, language_code, context, model_name, audio_file, engine, max_sleep_sec)
    time_method do
      threads = []
      100.times do |i| 
        threads << Thread.new do
          client = VoiseClientTest.new
          client.stream_recognize(encoding, sample_rate, language_code, context, model_name, audio_file, engine)
          client.close
          puts "Thread #{i+1} finnished"
        end

        # Sleep until 'max_sleep_sec' seconds
        sleep(rand * max_sleep_sec)
      end
      threads.each {|t| t.join }
    end
  end

  # VoiseClientTest.stream_test("me", "felicitacao/localizacao", "./Alo {duvida}.wav")
  def self.stream_test(engine, model_name, audio_file)
    time_method do
      client = VoiseClientTest.new
      100.times do |i| 
        client.stream_recognize("LINEAR16", 8000, "pt-BR", nil, model_name, audio_file, engine)
      end
      client.close
    end
  end

  # VoiseClientTest.stream_aborted_test("me", "felicitacao/localizacao", "./Alo {duvida}.wav")
  def self.stream_aborted_test(engine, model_name, audio_file)
    time_method do
      client = VoiseClientTest.new

      thr = Thread.new do
        sleep(1)
        client.close
        puts 'Client stopped'
      end

      client.stream_recognize("LINEAR16", 8000, "pt-BR", nil, model_name, audio_file, engine)
      
      thr.join
    end
  end

  # VoiseClientTest.stream_slow_test("me", "felicitacao/localizacao", "./Alo {duvida}.wav")
  def self.stream_slow_test(engine, model_name, audio_file)
    time_method do
      client = VoiseClientTest.new

      client.stream_recognize("LINEAR16", 8000, "pt-BR", nil, model_name, audio_file, engine) do |c, time|
        factor = 1

        total_bytes = 0
        while (buffer = IO.binread(audio_file, BYTES_PER_BUFFER, total_bytes)) != nil do 
          total_bytes += buffer.length

          c.data_streaming_recognize(buffer)

          factor *= 1.02

          # To simulate real-time audio, sleep after sending each audio buffer
          sleep(time * factor)
        end
      end
      
      client.close
    end
  end

  # VoiseClientTest.say_test("Mensagem de teste")
  def self.say_test(text)
    time_method do
      client = VoiseClientTest.new
      100.times do |i| 
        client.say(text, "alaw", 8000, "pt-BR", "result{i}.wav")
      end
      client.close
    end
  end

  # client = VoiseClientTest.new
  # client.recognize_from_file("FLAC", 16000, "pt-BR", nil, "felicitacao/localizacao", "./alo.flac")
  # client.recognize_from_file("LINEAR16", 8000, "pt-BR", nil, "felicitacao/localizacao", "./Alo {duvida}.wav")
  # client.recognize_from_file("LINEAR16", 8000, "pt-BR", nil, "felicitacao/localizacao", "./Sem voz {#NOVOICE}.wav")
  # client.recognize_from_fileexit("LINEAR16", 8000, "pt-BR", nil, "felicitacao/localizacao", "./ela_esta_no_trabalho.wav")
  def recognize_from_file(encoding, sample_rate, language_code, context, model_name, audio_file, engine = nil)
    VoiseClientTest::time_method do
      puts @client.recognize_from_file(encoding, sample_rate, language_code, context, model_name, audio_file, engine).to_pretty_s
    end
  end

  # client = VoiseClientTest.new
  # client.stream_recognize("FLAC", 16000, "pt-BR", nil, "felicitacao/localizacao", "./alo.flac")
  # client.stream_recognize("LINEAR16", 8000, "pt-BR", nil, "felicitacao/localizacao", "./Alo {duvida}.wav")
  # client.stream_recognize("LINEAR16", 8000, "pt-BR", nil, "felicitacao/localizacao", "./Sem voz {#NOVOICE}.wav")
  def stream_recognize(encoding, sample_rate, language_code, context, model_name, audio_file, engine = nil)

    response = @client.start_streaming_recognize(encoding, sample_rate, language_code, context, model_name, engine)

    # If the start was not accepted..
    if response.result.code != 201
      puts response.to_pretty_s
      return
    end

    samples_per_buffer = BYTES_PER_BUFFER / BYTES_PER_SAMPLE
    samples_per_millis = sample_rate / 1000

    sleep_milli = samples_per_buffer / samples_per_millis / 1000.0
    
    if block_given?
      yield(@client, sleep_milli)
    else
      total_bytes = 0

      while (buffer = IO.binread(audio_file, BYTES_PER_BUFFER, total_bytes)) != nil do 
        total_bytes += buffer.length

        @client.data_streaming_recognize(buffer)

        # To simulate real-time audio, sleep after sending each audio buffer
        sleep(sleep_milli)
      end
    end

    VoiseClientTest::time_method do
      reponse = @client.stop_streaming_recognize
      puts reponse.to_pretty_s
    end
  end

  # client = VoiseClientTest.new
  # client.say_to_file("Mensagem de teste", "LINEAR16", 8000, "pt-BR", "output.wav")
  # client.say_to_file("<prosody range=\"x-high\"><prosody pitch=\"x-high\">Mensagem de teste 2</prosody></prosody>", "LINEAR16", 8000, "pt-BR", "output2.wav")
  def say_to_file(text, encoding, sample_rate, language_code, filename)
    response = @client.say_to_file(text, encoding, sample_rate, language_code, filename)

    puts response.to_pretty_s
  end

  def close
    @client.close
  end

private

  def self.time_method(method = nil, *args)
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