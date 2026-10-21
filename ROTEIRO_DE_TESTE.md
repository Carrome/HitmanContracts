# Roteiro de teste — Hitman Contracts 1.2.0

> **1.2.0: campanha “O Último Contrato”.** A nova **Sessão 8** joga a campanha inteira: o Fixer, três capítulos, a traição e os dois finais.

> **1.1.0: consequências.** A nova **Sessão 7** testa o cliente traidor (não paga, polícia, matador), a vingança e o alvo assustado.

> **1.0.0: catálogo de contratos.** A nova **Sessão 6** testa os quatro contratos novos (Ligação, Orelhão, Passional, Distintivo), cada um com o seu botão "Postar: …" no menu Insert.

> **0.9.0: carreira e Fixer.** A nova **Sessão 5** testa os níveis de reputação, o Fixer (ligação, serviço, loja, ficha) e o contrato não letal de intimidação.

> **0.8.0: arsenal.** A nova **Sessão 4** testa as armas do jogo, as facas, o garrote e o veneno.

> **0.7.0: furtividade.**
> - O heat agora sobe quando o **corpo é achado** e quando uma **testemunha denuncia** (1–2 h depois). Os passos 11, 12, 12b e 26 mudaram.
> - A entrega mostra a **nota do serviço** (passo 28).
> - O menu Insert tem uma seção **Evidências** para forçar descoberta e denúncias.

> **0.5.7: menu de teste na tecla Insert.** Tudo o que as teclas F fazem também está lá, mais teletransportes (anúncio, cliente, alvo, encontro, policial, orelhão), dinheiro, vida, tempo e as chaves de debug do jogo. Use à vontade para pular deslocamentos. Logo no passo 5, abra e feche o menu uma vez (itens M1 e M2 do checklist).

Siga na ordem. Cada passo tem o que fazer, o que deve acontecer (✔) e, quando importa, o que fazer se falhar (✖). O código entre colchetes, como **[W3]**, é o item do `CHECKLIST_TESTES.md`: anote ali ✅, ❌ ou ⚠️ com uma linha do que viu.

> **0.5.6:** a **tranquilizante** foi corrigida: 3 minutos reais de sono, sem perseguição e sem ícone de olho. Repita o passo **10** com atenção.
>
> **O que mudou desde o seu teste anterior (0.5.4 → 0.5.5)**
> - Já passaram: pistolas, heat subindo nas mortes, contrato até a morte do alvo. Os passos 8–12 podem ser rápidos.
> - **Corrigida a entrega do contrato.** Depois de aceitar, o mod não achava a pergunta "matar" (o jogo a move para o caso), e por isso a entrega não validava. Repita os passos **25–28** até receber o dinheiro.
> - **O policial agora fala** na ligação (passo 33) e as opções de pacote funcionam (passo 35).
> - **Heat mais curto:** níveis em **30 / 60 / 90** (antes 50/100/160).
> - **Novo: interrogatório.** No nível 2, um Enforcer vem até a sua casa e faz 2 perguntas (passos 34a–34d). A ronda com falas de rádio acabou.
> - O **F8** agora avisa "Contrato criado" e, quando o anúncio vai para o quadro, o local certo (não mais "?").
> - **Comece uma cidade nova** (passo 2).

**Antes de começar**
- Deixe o celular à mão para anotar.
- As teclas de teste só funcionam com a janela do jogo em foco.
- 🛑 marca um ponto de parada: se aquilo falhar, pare a sessão e me avise, porque o resto depende disso.
- No fim: feche o jogo e **não abra de novo** antes de eu ler o log.

| Tecla | Ação | Tecla | Ação |
|---|---|---|---|
| F2 | dump | F9 | resumo na tela |
| F3 | as duas pistolas | F10 / F11 | matar o alvo do contrato |
| F4 | o policial liga | Shift+F6 / Ctrl+F6 | heat +10 / −10 |
| F8 | postar contrato | Secundário / Primário | mirar / atirar |
| F12 | Enforcers vêm te interrogar | **Insert** | **menu de teste** |

---

## Sessão 1 — Pistolas e contrato completo (~60 min)

### Menu principal

- [ ] **1.** Abra o jogo normalmente. **[A1, A2]**
  ✔ Chega ao menu principal sem tela preta nem travamento.

### Jogo de teste

- [ ] **2.** Comece um **jogo novo em modo sandbox (sem a história)**, com **cidade pequena**. Se houver opção de dinheiro inicial, escolha bastante: você vai precisar para a loja e para o policial.
- [ ] **3.** Espere a cidade terminar de gerar. Pode levar alguns minutos. **[B1]**
- [ ] **4.** Salve imediatamente com o nome **`HITMAN 1 - inicio`**. Esse save limpo serve para voltar se algo der errado.

### Ferramentas

- [ ] **5.** Aperte **F9**.
  ✔ Aparece "[Dev] Nenhum contrato ativo…", "[Dev] Heat 0 pts…" e uma linha "[Dev] Policial: (nome)…".
  ✖ Se nada aparecer, anote e siga. As outras teclas dizem se o mod está vivo.
- [ ] **5b.** Aperte **Insert**. **[M1, M2]**
  ✔ O menu de teste abre à direita, com o mouse livre e o status no topo (hora, dinheiro, heat, contrato, policial).
  - Aperte **Insert** de novo. ✔ O menu fecha e a câmera e o movimento voltam ao normal.
- [ ] **6.** Aperte **F8 três vezes**. 🛑 **[B2]**
  ✔ A cada vez aparece "[Dev] Contrato #N criado…" e, logo depois, "[Dev] Contrato #N postado em (local)", com o nome do lugar (não "?").
  - Se aparecer "O jogo não achou cliente e alvo para um novo 'HitmanPizza' agora", aperte F8 de novo. Anote quantas vezes isso aconteceu.
  ✖ Se aparecer "O jogo não conhece o job 'HitmanPizza'", pare a sessão e me avise.
- [ ] **7.** Aperte **F9** e anote, para cada contrato: o local do anúncio, o alvo e o cargo, o valor e se ele **aceitaria ou recusaria** suborno. **[B3]**
  - Escolha o **contrato principal**: de preferência um anúncio perto de você.

### Pistolas

- [ ] **8.** Aperte **F3**. 🛑 **[W1]**
  ✔ As duas pistolas aparecem no inventário, com os nomes "Lazarus 5 com silenciador" e "Lazarus 5 tranquilizante".
- [ ] **9.** Pegue uma delas. Segure o **secundário** (mirar) e aperte o **primário** (atirar) várias vezes contra uma parede. **[W2]**
  ✔ A câmera aproxima, os avisos mudam para "Mirar"/"Atirar", sai tiro com marca na parede, a munição nunca acaba e não há recarga.
  ✔ Sem mirar, o primário dá uma pancada normal.
- [ ] **10.** Com a **tranquilizante**, acerte um NPC qualquer. **Não pode ser** o alvo nem o cliente de um contrato. Comece a cronometrar no celular. 🛑 **[W3]**
  ✔ Ele cai dormindo e não morre. Ninguém começa a te perseguir, você não fica procurado, e o ícone de olho não fica na tela.
  ✔ Ele acorda depois de **uns 3 minutos** (sem contar o tempo em pause) e volta à rotina, **sem vir atrás de você**.
  ✖ Se ele dormir muito mais que isso, ou se o ícone de olho ficar, anote o tempo e me avise.
- [ ] **11.** Com a **letal**, acerte um NPC sozinho, sem ninguém olhando (beco, rua vazia de noite). Aperte **F9**. 🛑 **[W4, V1]**
  ✔ Ele morre com um tiro. **Desde a 0.7 o heat não sobe na hora**: o F9 mostra "Mortes 1", heat 0 e "corpos não achados 1".
  - Olhe o corpo de perto. Anote se ele aparece como um cadáver normal do jogo, que dá para examinar.
  - **Espere alguém achar o corpo** (ou use "Achar corpo escondido" no menu Insert). ✔ Aparece "O corpo de (nome) foi encontrado. (heat +20)" e os Enforcers vão ao local (cena de crime). **[V2]**
- [ ] **12.** Com a **letal**, mate alguém na frente de outras pessoas (bar, lanchonete). **[W5, V3]**
  ✔ Aparece "(nome) viu tudo. Em cerca de 1–2 h vai à polícia…" (ou "N pessoas viram…"). Se havia câmera ligada te vendo, aparece também "Uma câmera de segurança registrou você".
  - Deixe passar 1–2 h de jogo. ✔ Para cada testemunha aparece "(nome) contou à polícia o que viu. (heat +15)".
  - Se der problema com a polícia, fuja ou recarregue o save. O importante é anotar as mensagens e o heat.
- [ ] **12b. Silenciar uma testemunha.** Repita o passo 12 e, antes de 1 h, **fale com uma das testemunhas**. **[V4]**
  ✔ Aparece a opção "Você não viu nada, certo? Toma Cr200 pra ajudar a esquecer." Ela aceita (e não denuncia) ou recusa (denuncia na hora e foge).
  - Outra testemunha: acerte-a com o **dardo**. ✔ Aparece "(nome) vai acordar sem lembrar do que viu." e ela não denuncia. **[V5]**
- [ ] **13.** Aperte **Ctrl+F6** até o heat voltar a **0**, para não atrapalhar o resto.

### Contrato principal

- [ ] **14.** Vá até o quadro do contrato principal e leia o bilhete. **[C1]**
  ✔ O bilhete diz "CONTRATA-SE / Free Lancer para delivery de pizza / (valor) / Ligue para (número)", em português. Anote o valor.
- [ ] **15.** **Aceite** o job no bilhete.
- [ ] **16.** Ligue para o número do bilhete, de qualquer telefone (orelhão ou de casa). **[C2]**
  ✔ O cliente fala o local, a **hora** e o item na mão. Aparece também "Encontro com o cliente às HH:MM. Tolerância até HH:MM."
  ✖ Se a hora aparecer como `??:??` ou o texto estiver estranho, tire print. A mensagem na tela ainda serve de guia.
- [ ] **17.** Salve como **`HITMAN 2 - antes do encontro`**.
- [ ] **18.** Vá ao local **antes do horário limite**, ache o cliente com o item e fale com ele. **[C3]**
  ✔ A fala do cliente vem com o nome do alvo, o motivo e o local de trabalho, e aparece "Detalhes do serviço atualizados".
  ✔ Aparece "O cliente adiantou Cr…" e metade do valor entra na sua carteira.
  - Veja no quadro do caso se o alvo aparece com nome e local de trabalho.
  - Anote se ele te entregou uma maleta ou um dossiê.
- [ ] **19.** Investigue até achar o alvo: local de trabalho, ficha de funcionários etc. **[C4]**
- [ ] **20.** Antes de chegar no alvo, converse com **outra pessoa qualquer**. **[S1]**
  ✔ A opção "Alguém me pagou Cr… pra te apagar…" aparece. Escolha.
  ✔ A pessoa diz "Do que você tá falando?…" e nada mais acontece.
- [ ] **21.** Perto do alvo, salve como **`HITMAN 3 - perto do alvo`**.
- [ ] **22.** **Suborno:** fale com o alvo e escolha a opção.
  - Se o F9 disse "aceitaria" **[S2]**: ✔ ele paga o dobro, aparece "Suborno aceito…" e o contrato fecha, com o alvo vivo.
  - Se disse "recusaria" com cargo comum **[S3]**: ✔ ele grita por socorro e foge.
  - Se disse "recusaria" com cargo de luta **[S4]**: ✔ ele te ataca.
- [ ] **23.** Carregue **`HITMAN 3 - perto do alvo`** para voltar a antes do suborno.
- [ ] **24.** **Dardo no alvo:** acerte o alvo com a **tranquilizante**. **[C8]**
  ✔ Ele dorme, e o contrato **não** é cumprido (nenhuma mensagem de "Contrato cumprido").
- [ ] **25.** Com ele ainda dormindo, atire com a **letal**. 🛑 **[C5]**
  ✔ Ele morre e aparece "Contrato cumprido. Preencha o formulário de resolução e entregue o caso." O caso continua aberto e o objetivo "Mate (nome)" fica completo.
  - Abra o caso: a pergunta do formulário sobre o alvo deve aparecer como **cumprida**.
  ✖ Se não morrer, aperte **F10** perto dele, e **F11** se o F10 não matar. Anote qual funcionou.
  ⚠️ **Não use "fechar caso"** na tela de resolução: isso encerra o job sem pagamento.
- [ ] **26.** Aperte **F9** e anote o heat e as evidências. **[H1]**
  ✔ O heat **não subiu ainda**: ele sobe **10** (+5 na rua, +10 com câmera) quando o corpo for achado, e +15 por testemunha que denunciar.
  - Se der tempo, esconda o corpo (arraste-o para um canto onde ninguém passe) e veja se ele continua "não achado" no F9.
- [ ] **27.** Salve como **`HITMAN 4 - alvo morto`**, carregue esse save e aperte **F9**. **[C9, W7]**
  ✔ O alvo continua morto, o contrato continua cumprido e o heat não subiu de novo.
- [ ] **28.** Faça a entrega como num side job normal: formulário de resolução e entrega por baixo da porta do cliente. 🛑 **[C6]**
  ✔ Você recebe o resto do dinheiro: o valor do bilhete menos o sinal que o cliente pagou no encontro (passo 18).
  ✔ **Novo (0.7):** aparece "Avaliação do serviço: Assassino Silencioso / Profissional / Desleixado", com bônus nos dois primeiros. Anote qual veio e por quê (testemunha? câmera? corpo achado? outra morte?). **[V6]**
  ✖ Se o jogo não deixar entregar, tire print da tela do caso e do formulário e me avise.

### Atraso

- [ ] **29.** Carregue **`HITMAN 2 - antes do encontro`**. **Não** vá ao encontro: vá para casa e durma ou espere até passar o horário limite. **[C7]**
  ✔ Aparece "Você não apareceu no encontro. O cliente cancelou o contrato." e o caso falha.

**Fim da sessão 1.** Se for parar aqui, feche o jogo e me avise.

---

## Sessão 2 — Heat e policial corrupto (~45 min)

Carregue **`HITMAN 4 - alvo morto`** (ou continue de onde parou, mas sem contrato em andamento).

- [ ] **30.** Vá para o **seu apartamento** e fique lá.
- [ ] **31.** Aperte **Shift+F6** até passar de **30**. **[H2]**
  ✔ Aparece "Os jornais estão falando de um matador de aluguel… (Heat nível 1)".
- [ ] **32.** Continue até passar de **60**. **[H3]**
  ✔ Aparece "Os Enforcers estão investigando as mortes… (Heat nível 2)".
- [ ] **33.** Espere uns **15 minutos de jogo** em casa: o telefone deve tocar. Atenda. Se não tocar, aperte **F4**. 🛑 **[P1]**
  ✔ **Ele fala** pelo telefone: se apresenta da delegacia e dita um número. O número aparece no caderno como "Contato na polícia".
  ✖ Se ele ficar mudo de novo, anote e siga (o resto da sessão ainda funciona).
- [ ] **34.** Continue em casa. Em até **1 hora de jogo** depois do passo 32, os Enforcers vêm para o interrogatório (se não quiser esperar, aperte **F12**). 🛑 **[I1]**
  - **34a.** ✔ Um Enforcer chega perto de você e diz algo como "Polícia. Estamos investigando umas mortes no bairro… Onde você estava (dia), por volta das (hora)?". A mesma frase aparece na tela, junto com "Fale com o Enforcer para responder."
  - **34b.** Fale com ele. ✔ Aparecem 3 respostas: "Em casa, dormindo." / "Trabalhando num caso, do outro lado da cidade." / "Não é da sua conta." Escolha uma. **[I2]**
  - **34c.** ✔ Ele reage e faz a 2ª pergunta: "Você conhecia (nome da última vítima)?". Se as respostas não aparecerem sozinhas, fale com ele de novo. Escolha: "Nunca ouvi falar." / "Só de vista, do bairro." / "Quero um advogado." **[I3]**
  - **34d.** ✔ Ele fecha com uma frase final e aparece o resultado: "O Enforcer acreditou em você (heat −10)", "…nem se convenceu nem desconfiou (heat igual)" ou "…desconfiou de você (heat +10/+20)". Os Enforcers vão embora, **sem revistar** a casa. **[I4]**
  - Anote as respostas que escolheu e o resultado.
- [ ] **35.** Ligue para o número do policial, do telefone de casa. **[P3]**
  ✔ Ele diz "Fala. O que você quer que suma?" e aparecem 3 opções com pontos e preço.
  - Escolha **"Some com uma morte"**. ✔ Ele diz em qual orelhão deixar o envelope e até que horas.
- [ ] **36.** Vá até esse orelhão com dinheiro suficiente e chegue bem perto dele. **[P4]**
  ✔ Aparece "Você deixou o envelope…", o dinheiro sai e o heat cai 10 (confira no F9).
- [ ] **37.** Aperte **Shift+F6** até passar de **90**. **[H4]**
  ✔ Aparece "Há um mandado contra você…".
  - Entre num prédio qualquer: os Enforcers e seguranças te tratam como procurado?
  - Espere até 2 horas de jogo: aparece "Você sente que está sendo seguido."? Alguém vem atrás de você?
- [ ] **38.** Com o heat acima de 90, ligue para o policial. **[P5]**
  ✔ Os preços estão no **dobro** dos do passo 35.
  - Desligue sem escolher.
- [ ] **39.** Aperte **Ctrl+F6** até ficar abaixo de 90. **[H5]**
  ✔ Aparece "Heat reduzido para o nível 2". O caçador para de te seguir?
- [ ] **40.** Durma até a **manhã seguinte** e pegue um jornal (banca, lanchonete). **[H2–H4]**
  ✔ A primeira página tem uma das manchetes do mod ("MATADOR DE ALUGUEL À SOLTA?", "ENFORCERS FECHAM O CERCO…" ou "MANDADO EXPEDIDO…"). Tire print.
- [ ] **41.** Salve, carregue e aperte **F9**. **[H6, P8]**
  ✔ O mesmo heat, o mesmo policial e o número dele continua funcionando.

**Fim da sessão 2.**

---

## Sessão 3 — Extras (opcional, ~30 min)

- [ ] **42. Loja.** Encontre um **vendedor de armas** ou o **mercado negro** e veja se as duas pistolas estão à venda. Anote os preços. **[W6]**
- [ ] **43. Orelhão perto de você.** Carregue **`HITMAN 1 - inicio`**. Longe de casa e **perto de um orelhão**, suba o heat a 60 e aperte **F4**. **[P2]**
  ✔ Primeiro toca em casa (você não ouve). Depois toca o **orelhão perto de você**.
- [ ] **44. Prazo do envelope.** Escolha um pacote com o policial e **não** vá ao orelhão. Durma mais de 24 h. **[P6]**
  ✔ Aparece "O policial cansou de esperar…".
- [ ] **45. Side job comum.** Pegue e complete um side job normal do jogo. **[D1, D2]**
  ✔ Funciona e paga normal.
- [ ] **46. Suborno de novo.** Se no passo 22 só deu para testar um resultado, repita com outro contrato do passo 6 cujo alvo dê o outro resultado. **[S2–S4]**
- [ ] **47. Fugir do interrogatório.** Aperte **F12** em casa e, quando o Enforcer começar a falar, **saia do prédio**. **[I5]**
  ✔ Aparece "Você saiu no meio do interrogatório. (heat +10)".
- [ ] **48. Não estar em casa.** Com o heat entre 60 e 89, fique **longe de casa** por mais de 1 dia de jogo. **[I6]**
  ✔ Aparece "Os Enforcers passaram no seu apartamento várias vezes e nunca te encontraram… (heat +10)".

---

## Sessão 4 — Arsenal (novo na 0.8, ~30 min)

Use um save de teste. No menu **Insert → Armas**, "Dar um item do arsenal" entrega um item por clique (revólver, espingarda, rifles, facas, katana, corda, arame, seringa, frasco de veneno).

- [ ] **49. Revólver.** Mire e atire em alguém numa rua com gente. **[X1]**
  ✔ Ele morre, o tiro faz barulho e clarão, e aparece "(N) pessoas viram tudo…", contando também quem só ouviu.
- [ ] **50. Rifle de sniper.** Mire de longe. **[X2]**
  ✔ A câmera aproxima bem mais que a pistola. Anote se dá para acertar de longe.
- [ ] **51. Faca ou katana.** Ataque alguém com a faca na mão. **[X3]**
  ✔ Ele morre no golpe que o derruba ou no 2º golpe.
- [ ] **52. Garrote.** Com a corda, chegue **por trás** de alguém que não te vê e ataque. **[X4]**
  ✔ Ele morre na hora, sem grito. De frente, é só um golpe comum.
- [ ] **53. Veneno.** Adormeça alguém com o **dardo** e use a **seringa** nele. **[X5]**
  ✔ Aparece "Você envenenou (nome)…". Ele acorda depois de uns 3 minutos e morre 1–3 h de jogo depois.
  ✔ Quando acham o corpo: "Parece morte natural. (heat +2)", sem Enforcers no local.
- [ ] **54. Lojas.** No vendedor de armas ou no mercado negro, o revólver, a espingarda e os rifles estão à venda? **[X6]**

---

## Sessão 5 — Carreira e Fixer (novo na 0.9, ~40 min)

- [ ] **55. Primeira reputação.** Entregue um contrato (passo 28) ou use **Insert → Carreira → "+5 de reputação"**. **[K1]**
  ✔ Depois da nota aparece "Sua reputação subiu: agora você é Profissional…" (com +5 ou após alguns serviços).
- [ ] **56. O Fixer liga.** Fique em casa. Em até meia hora de jogo o telefone toca (ou use "Fixer liga agora"). Atenda. **[K2]**
  ✔ Ele se apresenta ("Aqui é (nome)… Anota: (número)") e o número vai para o caderno como "Fixer".
- [ ] **57. Serviço do Fixer.** Ligue para o número dele. **[K3]**
  ✔ Ele atende "Fala, (nível). O que vai ser?" e aparecem as opções.
  - Escolha **"Tem serviço pra mim?"**. ✔ "Tenho um. Paga bem…" e logo aparece "O Fixer deixou o serviço num quadro: (lugar)". Anote o valor do contrato.
- [ ] **58. Comprar.** Ligue de novo e compre a **seringa** e o **bilhete de suicídio**. **[K4]**
  ✔ O dinheiro sai e os itens chegam ao inventário.
- [ ] **59. Ficha.** Ligue e escolha "Como anda a minha fama?". **[K5]**
  ✔ Ele lista nível, reputação, serviços, mortes, ganhos e heat.
- [ ] **60. Intimidação.** Use **Insert → Contratos → "Postar: Intimidação"** e siga o contrato: ligue, encontre o cliente, dê uma **surra** no alvo (soco ou bastão, sem matar) e entregue. **[K6]**
  ✔ O cliente pede "uma boa surra, nada de matar". A entrega paga cerca de 40% de um assassinato.
- [ ] **61. Suicídio forjado.** Leve o bilhete de suicídio para a casa de um alvo e largue-o lá dentro. Depois adormeça o alvo com o dardo e use a seringa (ou garrote por trás). **[K7]**
  ✔ Quando acharem o corpo: "Parece suicídio. (heat +0)".

---

## Sessão 6 — Catálogo de contratos (novo na 1.0, ~60 min)

Use um save de teste. Cada contrato tem o seu botão em **Insert → Contratos → "Postar: …"**, que funciona até acima do seu nível. Para pular deslocamentos, use "Ir ao anúncio", "Ir ao cliente" e "Ir ao alvo". Se algum "Postar" responder "found no client/target pair", anote qual e siga para o próximo.

- [ ] **62. Ligação.** Clique "Postar: Ligação", vá ao anúncio, aceite e ligue para o número. **[N1]**
  ✔ O bilhete diz "PRECISA-SE / Resolvedor de problemas, discreto".
  ✔ Na ligação, o cliente diz o nome do alvo, que ele passou a perna no cliente, onde trabalha e onde entregar.
  ✔ Aparecem "Detalhes do serviço atualizados" e o sinal de 50% (o dinheiro sobe).
- [ ] **63. Orelhão.** Clique "Postar: Orelhão", aceite e ligue. **[N2]**
  ✔ O cliente marca um orelhão e uma hora.
  - Vá até o orelhão e atenda quando tocar.
  ✔ A voz diz "Não diga nada. Só escute…" e dita o alvo. Depois vêm o aviso de detalhes e o sinal.
  - Anote se apareceu uma maleta no orelhão.
- [ ] **64. Passional.** Clique "Postar: Passional", aceite, ligue e vá ao encontro na hora marcada. **[N3]**
  ✔ O cliente diz que o alvo o trai. O alvo é o parceiro ou o amante do cliente?
- [ ] **65. Distintivo.** Clique "Postar: Distintivo", aceite, ligue e vá ao encontro. **[N4]**
  ✔ O alvo é Enforcer, segurança ou cobrador, e o valor é maior que o da pizza.
  - Ataque-o. Ele reage como quem sabe brigar?
- [ ] **66. Entrega.** Termine um dos quatro: mate o alvo, preencha o caso e entregue por baixo da porta do cliente. **[N5]**
  ✔ O caso valida, o resto do pagamento chega e aparece a nota.
- [ ] **67. Níveis (opcional, demora).** Num save novo, com reputação 0, espere um dia de jogo. **[N6]**
  ✔ Nos quadros aparecem só a pizza e a intimidação.
  - Use "+5 de reputação" 1, 3 e 6 vezes, esperando entre uma e outra. Os outros contratos vão aparecendo pela tabela da seção N do checklist.
- [ ] **68. Fixer de nível alto.** Com 30+ de reputação, peça "Tem serviço pra mim?" ao Fixer. **[N7]**
  ✔ Ele posta o contrato Distintivo (o de nível mais alto).

---

## Sessão 7 — Consequências (novo na 1.1, ~40 min)

Use um save de teste em que você já tenha **entregue pelo menos um contrato**, porque o cliente nunca trai no primeiro serviço. Com o menu, isso não importa: os botões forçam a traição. Para cada entrega, use um contrato rápido (Ligação ou Pizza) e o "Matar alvo: tiro (F10)" para não perder tempo.

- [ ] **69. Cliente que não paga.** Clique **Insert → Consequências → "Próxima entrega: não paga"**. Faça e entregue um contrato. **[T1]**
  ✔ "(cliente) ficou com o resto do seu dinheiro (CrX) e sumiu…". O dinheiro não sobe na entrega.
- [ ] **70. Cobrar.** Clique "Ir ao cliente" e mate-o. **[T2]**
  ✔ "Dívida cobrada: … CrX são seus." O dinheiro sobe.
- [ ] **71. Polícia na entrega.** Clique "Próxima entrega: polícia" e entregue outro contrato. **[T3]**
  ✔ "(cliente) pagou, mas chamou a polícia para você. Saia daí! (heat +15)". Os Enforcers chegam.
- [ ] **72. Matador do cliente.** Clique "Próxima entrega: caçador" e entregue mais um contrato. Depois use "Adiantar 24 h" (duas vezes, se preciso). **[T4]**
  ✔ "(cliente) não quer pontas soltas…" e "Você sente que está sendo seguido." Alguém vem atrás de você.
- [ ] **73. Vingança.** Clique "Vingança agora". **[T5]**
  ✔ No minuto seguinte, a mensagem aparece e um matador vem atrás de você.
- [ ] **74. Alvo assustado.** Aceite um contrato e clique "Assustar o alvo atual". Espere uns minutos de jogo e use "Ir ao alvo". **[T6]**
  ✔ "(alvo) ficou com medo e vai se trancar em casa…", e ele está em casa.
- [ ] **75. Save.** Com uma dívida ou um matador a caminho, salve, saia para o menu e carregue. Clique "Dívidas e caçadores". **[T7]**
  ✔ Continua tudo lá.

---

## Sessão 8 — Campanha “O Último Contrato” (novo na 1.2, ~60 min)

Fique **em casa** quando for esperar uma ligação: os dois ligam para o telefone do seu apartamento. Para ir mais rápido, use **Insert → Campanha**.

- [ ] **76. O Fixer liga.** Em casa, clique "Começar (o Fixer liga)" e atenda. **[L1]**
  ✔ "Apareceu um cliente grande… Me liga e pergunta pelo cliente grande." e depois "Nova história: “O Último Contrato”…".
- [ ] **77. Capítulo 1.** Ligue para o Fixer pelo número do caderno e escolha "Sobre o cliente grande…". **[L2]**
  ✔ Ele explica o teste, e aparece em que quadro está o bilhete.
- [ ] **78. Fazer o capítulo.** Vá ao bilhete, aceite, faça e entregue (use F10 no alvo se quiser pular). **[L3]**
  ✔ "capítulo 1 concluído. (Fixer) vai ligar…". Volte para casa: ele liga em ~1 h de jogo.
- [ ] **79. Capítulos 2 e 3.** Faça os dois, ou clique "Pular capítulo" duas vezes, atendendo as ligações em casa. **[L4]**
  ✔ Anote o valor de cada capítulo, comparado com um contrato normal.
- [ ] **80. A traição.** Depois do capítulo 3 (ou com "Traição agora"), o policial liga. Atenda. **[L5]**
  ✔ Ele avisa que o Fixer te vendeu. Aparecem a escolha e "Você sente que está sendo seguido.".
- [ ] **81. Final B.** Salve antes (para testar o outro final depois). Ligue para o policial e escolha "Quero entregar (Fixer).". **[L6]**
  ✔ O final aparece e o heat vai a 0.
- [ ] **82. Final A.** Carregue o save do passo 81, use "Ir ao Fixer" e mate-o. **[L7]**
  ✔ O final aparece e você recebe Cr5.000.
- [ ] **83. Save no meio.** Em algum capítulo, salve e carregue. A linha "Campanha" do menu continua na mesma etapa? **[L8]**

---

## Ao terminar

1. Feche o jogo.
2. **Não abra de novo.**
3. Me mande as anotações (os códigos com ✅, ❌ ou ⚠️) e os prints. Eu leio o log e o dump direto do PC.
