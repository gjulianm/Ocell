=== Explanations

*Why am I using a static Dependency class instead of real Dependency Injection?*

Because passing all dependencies on the constructor of each ViewModel would be a real hassle. I've created the DependencyModule class so the library user only has to add one line to his code. This class also avoids changing the user code whenever the dependencies change.