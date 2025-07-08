namespace RPG.global;

public interface IContainer<out T> {

    public int GetSize();
    public T GetAt(int pIndex);
    
}