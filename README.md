# Endless Runner
L'obiettivo del progetto era quello di riportare nella Realtà Aumentata un endless game già realizzato e funzionante in Unity utilizzando AR Foundation.

![](.readme/final-frontier.png)

## Implementazione

Funzionalità già implementate nel gioco:

- Menù di gioco
- Movimento orizzontale WASD
- Salto, rotazione, lancio abilità e score
- Generazione nuove piattaforme
- Eliminazione piattaforme
- Restart game

![image](https://user-images.githubusercontent.com/108952275/183376043-fcdd954d-7d46-4d63-a945-39ca5f78fda0.png)

Il video seguente mostra la generazione continua del mondo:

![Platform generation video](.readme/early-animation.webp)

Esempio del gioco:

![Gameplay video](.readme/late-game.webp)

Funzionalità da noi implementate: 
- Swipe per il movimento orizzontale e per il salto
- Double Tap per l’abilità
- Divisione in zone per la rotazione

![image](https://user-images.githubusercontent.com/108952275/183377183-11412a46-208a-4eb1-a628-528b7eab0390.png)

Soluzioni per portare il gioco in Realtà Aumentata:

- Utilizzare le coordinate della piattaforma iniziale come punto di partenza per la generazione del mondo
- Resize intero mondo e player
- Fermare la generazione dei piani per evitare sovrapposizione plane e piattaforme
- Aggiunta di un piano per triggerare la morte
- Rilancio AR Session dopo che il player muore ma ha ancora vite a disposizione

Inoltre è stato risolto il bug del gioco originale relativo alla possibiltà di saltare più volte.

Esempio gioco in realtà aumentata:

![image](https://user-images.githubusercontent.com/108952275/183377419-486eebec-3027-4b95-a631-427609209c8a.png)
