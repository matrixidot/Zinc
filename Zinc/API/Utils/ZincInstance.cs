namespace Zinc.API.Utils;

public class ZincInstance(ZincClass clazz) {
    private ZincClass Clazz = clazz;

    public override string ToString() => clazz.Name + " instance";


}