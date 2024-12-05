
def x(CB):
    return CB / (1900 - 18 * CB)

def inKelvin(y):
    return y + 273.15

def Temp(Ti):
    T0 = 273.15
    T = inKelvin(Ti)
    return (T - T0) / T ** 2

def n(Ti,CB):
    return 10 ** (-1.52 + ((0.065 + x(CB)) / (19.147 * inKelvin(Ti))) * (140845 - 4.4429 * (10 ** 7) * (Temp(Ti))))


for T in range(20, 55, 5):
    for CB in range(0, 60, 10):
        print(n(T, CB), T, CB)


print((n(40,0) - 0.65), (n(50,50) - 4.94), (n(30,30) - 2.5))


