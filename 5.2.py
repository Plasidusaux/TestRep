T0 = 273.15

x = lambda CB: CB / (1900 - 18 * CB)

inKelvin = lambda T: T + 273.15

Temp = lambda T: (inKelvin(T) - T0) / inKelvin(T) ** 2

T_list = []
CB_list = []
for T in range(20, 55, 5):
    for CB in range(0, 60, 10):
        T_list.append(T)
        CB_list.append(CB)


n = list(map(lambda T, CB: 10 ** (-1.52 + ((0.065 + x(CB)) / (19.147 * inKelvin(T))) * (140845 - 4.4429 * (10 ** 7) * (Temp(T)))), T_list, CB_list))

for x in range(0, len(CB_list)):
    print(n[x], T_list[x], CB_list[x])