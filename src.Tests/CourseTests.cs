using MiaLearningSystem;

public class CourseTests
{
    public CourseTests()
    {
        if (Directory.Exists("Data")) Directory.Delete("Data", recursive: true);
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

    [Fact]
    public void Test_UpdateCourse()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Old Name", "Science");
        dm.AddCourse(course);
        dm.UpdateCourse(course.CourseId, "New Name", "Biology");
        Assert.Equal("New Name", dm.Courses[0].Name);
        Assert.Equal("Biology", dm.Courses[0].SubjectArea);
    }

    [Fact]
    public void Test_PersistsCourses()
    {
        var dm1 = new DataManager();
        dm1.AddCourse(new Course(Guid.NewGuid(), "Spanish 101", "Language"));
        dm1.AddCourse(new Course(Guid.NewGuid(), "Biology 201", "Science"));

        var dm2 = new DataManager();
        Assert.Equal(2, dm2.Courses.Count);
        Assert.Contains(dm2.Courses, c => c.Name == "Spanish 101");
        Assert.Contains(dm2.Courses, c => c.Name == "Biology 201");
    }
}
