using MiaLearningSystem;

public class ReferenceListTests
{
    public ReferenceListTests()
    {
        if (Directory.Exists("Data")) Directory.Delete("Data", recursive: true);
    }

    [Fact]
    public void Test_AddReferenceList()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Spanish 101", "Language");
        dm.AddCourse(course);
        dm.AddReferenceList(new ReferenceList(Guid.NewGuid(), course.CourseId, "Vocabulary"));
        Assert.Equal(1, dm.ReferenceLists.Count);
    }

    [Fact]
    public void Test_UpdateReferenceList()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Spanish 101", "Language");
        dm.AddCourse(course);
        var rl = new ReferenceList(Guid.NewGuid(), course.CourseId, "Old Name");
        dm.AddReferenceList(rl);
        dm.UpdateReferenceList(rl.ReferenceListId, "New Name");
        Assert.Equal("New Name", dm.ReferenceLists[0].Name);
    }

    [Fact]
    public void Test_RemoveReferenceList_CascadesEntries()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Spanish 101", "Language");
        dm.AddCourse(course);
        var rl = new ReferenceList(Guid.NewGuid(), course.CourseId, "Vocabulary");
        dm.AddReferenceList(rl);
        dm.AddReferenceEntry(new ReferenceEntry(Guid.NewGuid(), rl.ReferenceListId, "Hola", "Hello"));
        dm.AddReferenceEntry(new ReferenceEntry(Guid.NewGuid(), rl.ReferenceListId, "Adiós", "Goodbye"));
        Assert.Equal(2, dm.ReferenceEntries.Count);
        dm.RemoveReferenceList(rl);
        Assert.Equal(0, dm.ReferenceLists.Count);
        Assert.Equal(0, dm.ReferenceEntries.Count);
    }

    [Fact]
    public void Test_AddReferenceEntry()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Spanish 101", "Language");
        dm.AddCourse(course);
        var rl = new ReferenceList(Guid.NewGuid(), course.CourseId, "Vocabulary");
        dm.AddReferenceList(rl);
        dm.AddReferenceEntry(new ReferenceEntry(Guid.NewGuid(), rl.ReferenceListId, "Hola", "Hello"));
        Assert.Equal(1, dm.ReferenceEntries.Count);
    }

    [Fact]
    public void Test_RemoveReferenceEntry()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Spanish 101", "Language");
        dm.AddCourse(course);
        var rl = new ReferenceList(Guid.NewGuid(), course.CourseId, "Vocabulary");
        dm.AddReferenceList(rl);
        var entry = new ReferenceEntry(Guid.NewGuid(), rl.ReferenceListId, "Hola", "Hello");
        dm.AddReferenceEntry(entry);
        Assert.Equal(1, dm.ReferenceEntries.Count);
        dm.RemoveReferenceEntry(entry);
        Assert.Equal(0, dm.ReferenceEntries.Count);
    }

    [Fact]
    public void Test_UpdateReferenceEntry()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Spanish 101", "Language");
        dm.AddCourse(course);
        var rl = new ReferenceList(Guid.NewGuid(), course.CourseId, "Vocabulary");
        dm.AddReferenceList(rl);
        var entry = new ReferenceEntry(Guid.NewGuid(), rl.ReferenceListId, "Hola", "Hello");
        dm.AddReferenceEntry(entry);
        dm.UpdateReferenceEntry(entry.EntryId, "Hola!", "Hello!");
        Assert.Equal("Hola!", dm.ReferenceEntries[0].Term);
        Assert.Equal("Hello!", dm.ReferenceEntries[0].Definition);
    }

    [Fact]
    public void Test_PersistsReferenceLists()
    {
        var dm1 = new DataManager();
        var course = new Course(Guid.NewGuid(), "Spanish 101", "Language");
        dm1.AddCourse(course);
        dm1.AddReferenceList(new ReferenceList(Guid.NewGuid(), course.CourseId, "Vocabulary"));
        dm1.AddReferenceList(new ReferenceList(Guid.NewGuid(), course.CourseId, "Grammar"));

        var dm2 = new DataManager();
        Assert.Equal(2, dm2.ReferenceLists.Count);
        Assert.Contains(dm2.ReferenceLists, r => r.Name == "Vocabulary");
        Assert.Contains(dm2.ReferenceLists, r => r.Name == "Grammar");
    }

    [Fact]
    public void Test_PersistsReferenceEntries()
    {
        var dm1 = new DataManager();
        var course = new Course(Guid.NewGuid(), "Spanish 101", "Language");
        dm1.AddCourse(course);
        var rl = new ReferenceList(Guid.NewGuid(), course.CourseId, "Vocabulary");
        dm1.AddReferenceList(rl);
        dm1.AddReferenceEntry(new ReferenceEntry(Guid.NewGuid(), rl.ReferenceListId, "Hola", "Hello"));
        dm1.AddReferenceEntry(new ReferenceEntry(Guid.NewGuid(), rl.ReferenceListId, "Adiós", "Goodbye"));

        var dm2 = new DataManager();
        Assert.Equal(2, dm2.ReferenceEntries.Count);
        Assert.Contains(dm2.ReferenceEntries, e => e.Term == "Hola");
        Assert.Contains(dm2.ReferenceEntries, e => e.Term == "Adiós");
    }
}
