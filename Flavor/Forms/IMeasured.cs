namespace Flavor.Forms {
    internal interface IMeasured {
        void initMeasure(bool isPrecise);
        void prepareControlsOnMeasureStart();
        void deactivateOnMeasureStop();
    }
}
