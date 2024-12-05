from prettytable import PrettyTable

def find(x):
    diap = []
    try:
        with open("input.txt", "r") as f:
            for line in f:
                if x in line:
                    a = line.split(' ')
                    a.pop(0)
                    for i in range(len(a)):
                        diap.append(int(a[i]))
                    return diap
    except Exception:
        print('No File or Wrong Data')
        exit()

CB = find('CB')
T = find('T')

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

my_table = PrettyTable()
my_table.field_names = ['n', 'T', 'CB']

for i in range(len(CB)):
    if CB[i] < 0:
        print('Wrong Data')
        exit()

for i in range(len(T)):
    if inKelvin(T[i]) < 0:
        print('Wrong Data')
        exit()

try:
    for i in range (CB[0], CB[1] + CB[2], CB[2]):
        for k in range (T[0], T[1] + T[2], T[2]):
            my_table.add_row([n(k, i), k, i])
except:
    print('Wrong Data')
    exit()

with open("output.txt", "w") as f:
    table = str(my_table)
    f.write(table)





