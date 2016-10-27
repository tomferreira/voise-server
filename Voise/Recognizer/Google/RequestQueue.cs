using Grpc.Core;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Voise.Recognizer.Google
{
    /// <summary>
    /// A queue for gRPC requests in a streaming API. gRPC only supports a single pending write
    /// at a time, but it's often useful for clients to be able to fire requests without waiting for them
    /// to complete.
    /// </summary>
    /// <typeparam name="T">Request type to write to the stream.</typeparam>
    public class RequestQueue<T>
    {
        private readonly IClientStreamWriter<T> _writer;
        private readonly BufferBlock<Tuple<T, bool>> _buffer;
        private readonly ActionBlock<Tuple<T, bool>> _target;

        public RequestQueue(IClientStreamWriter<T> writer, int capacity)
        {
            _writer = writer;
            // TODO: Check all options etc
            _buffer = new BufferBlock<Tuple<T, bool>>(new DataflowBlockOptions { BoundedCapacity = capacity });
            _target = new ActionBlock<Tuple<T, bool>>(WriteOrComplete);
            _buffer.LinkTo(_target, new DataflowLinkOptions { PropagateCompletion = true });
        }

        private Task WriteOrComplete(Tuple<T, bool> input)
        {
            return input.Item2 ? _writer.CompleteAsync() : _writer.WriteAsync(input.Item1);
        }

        // TODO: A version of this that returns a task which completes when the message is actually sent?
        // Unsure how that would be wired up.
        public void Post(T request)
        {
            var accepted = _buffer.Post(Tuple.Create(request, false));
            if (!accepted)
            {
                // TODO: Something prettier than this?
                throw new InvalidOperationException("Unable to send message");
            }
        }

        /// <summary>
        /// Completes the stream when the queue is drained.
        /// </summary>
        internal Task CompleteAsync()
        {
            // No need to await this, I think...
            _buffer.SendAsync(Tuple.Create(default(T), true));
            _buffer.Complete();
            return _buffer.Completion;
        }
    }
}
