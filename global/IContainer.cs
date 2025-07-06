namespace RPG.global;

public interface IContainer<T> {

    public int GetSize();
    public T GetAt(int pIndex);
    
}