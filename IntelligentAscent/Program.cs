using System;
using System.Diagnostics;
using System.Text;
using Tensorflow;
using static Tensorflow.Binding;

 
    /// <summary>
    /// Simple hello world using TensorFlow
    /// </summary>
public class HelloWorld  
    {
        public void Main()
        {
        int training_steps = 1000;
        float learning_rate = 0.01f;
        int display_step = 100;

        // We can set a fixed init value in order to demo
        var W = tf.Variable(-0.06f, name: "weight");
        var b = tf.Variable(-0.73f, name: "bias");
        var optimizer = Tensorflow.Keras.Optimizers.SGD(learning_rate);

        // Run training for the given number of steps.
        foreach (var step in range(1, training_steps + 1))
        {
            // Run the optimization to update W and b values.
            // Wrap computation inside a GradientTape for automatic differentiation.
            using var g = tf.GradientTape();
            // Linear regression (Wx + b).
            var pred = W * X + b;
            // Mean square error.
            var loss = tf.reduce_sum(tf.pow(pred - Y, 2)) / (2 * n_samples);
            // should stop recording
            // Compute gradients.
            var gradients = g.gradient(loss, (W, b));

            // Update W and b following gradients.
            optimizer.apply_gradients(zip(gradients, (W, b)));

            if (step % display_step == 0)
            {
                pred = W * X + b;
                loss = tf.reduce_sum(tf.pow(pred - Y, 2)) / (2 * n_samples);
                print($"step: {step}, loss: {loss.numpy()}, W: {W.numpy()}, b: {b.numpy()}");
            }
        }
    }
    }
 