Technical Implementation

Optimization

Object Pooling: Implemented for cards and audio effects to minimize garbage collection
Memory Management: Careful handling of resources with proper cleanup
Low Memory Handling: System detects and responds to low memory scenarios on mobile devices
Audio Optimization: Dynamic buffer size adjustment based on platform capabilities
Efficient Layout Calculation: Pre-calculation of layouts to avoid performance spikes


Animations

The animations are currently done from code since no external packages could be used but if we could, utilising DOTween for example would have done an excellent job.
