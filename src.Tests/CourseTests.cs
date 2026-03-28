using MiaLearningSystem;

public class CourseTests
{
    public CourseTests()
    {
        File.Delete("courses.txt");
        File.Delete("topics.txt");
        File.Delete("notes.txt");
        File.Delete("quizzes.txt");
        File.Delete("quiz_topics.txt");
        File.Delete("reference_lists.txt");
        File.Delete("reference_entries.txt");
    }

    [Fact]
    public void Test_AddCourse()
    {
        var dm = new DataManager();
        Assert.Equal(0, dm.Courses.Count);
        dm.AddCourse(new Course(Guid.NewGuid(), "Spanish 101", "Language"));
        Assert.Equal(1, dm.Courses.Count);
    }

    [Fact]
    public void Test_RemoveCourse()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Biology 201", "Science");
        dm.AddCourse(course);
        Assert.Equal(1, dm.Courses.Count);
        dm.RemoveCourse(course);
        Assert.Equal(0, dm.Courses.Count);
    }
}
