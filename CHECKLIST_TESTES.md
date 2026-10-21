# Checklist de testes — Hitman Contracts

**Versão instalada no jogo: 1.2.6** (atualizada em 20/10/2026; instala sozinha quando o jogo fechar)

> **1.2.6, o Fixer reformulado (decidido no 8º teste):**
> - **Telefone:** ligando para ele, o Fixer só diz **onde está agora** (casa, trabalho ou outro lugar, com o prédio) e pede que você vá até lá. O lugar também aparece na tela.
> - **Pessoalmente:** "Tem serviço pra mim?" passa o serviço **na hora**. Ele fala o alvo, o cargo e onde trabalha, e onde entregar. O caso já abre **aceito**, com as informações e o sinal de 50%. Não precisa achar bilhete nem ligar para número.
> - Os capítulos da campanha funcionam do mesmo jeito.
> - O teste anterior ainda era a 1.2.4: o jogo estava aberto, então a 1.2.5 nunca chegou a ser instalada.

> **Resultado do 8º teste (1.2.4) e o que mudou na 1.2.5:**
> - ❌ Pelo telefone, o policial e o Fixer ainda só mostravam as falas comuns.
>   - **Causa achada no código do jogo:** as opções de um cidadão ficam guardadas por "chave" (rosto, voz…). Cara a cara, o jogo lista as opções do **rosto** e da **voz**; pelo telefone, **só as da voz**. O SOD.Common prende todo diálogo do mod no rosto, por isso eles nunca entravam no telefone.
>   - **Corrigido:** as opções do policial, do Fixer e o "entregar" ficam presas na voz.
> - ⚠️ Pessoalmente, as opções do mod ficavam no fim da lista. **Corrigido:** o jogo ordena pelo "ranking" de cada opção, e as do mod ganharam o maior. Ficam em primeiro (interrogatório, suborno, testemunha, policial, Fixer).
> - Pessoalmente, as opções do Fixer não apareciam. Era a mesma causa, ou estavam lá no fim e escondidas. Teste de novo.

> **Resultado do 7º teste (1.2.3) e o que mudou na 1.2.4:**
> - ✅ Ligar para o policial e para o Fixer agora conecta, e eles falam a saudação (`[Phone] … takes the player's call` no log).
> - ❌ As opções deles apareciam por menos de 1 segundo e eram trocadas pelas falas comuns ("Olá, qual é seu nome?"…, título "Desconhecido Cidadão"). O jogo remonta essa conversa como se fosse cara a cara, e o modo cara a cara esconde opções de telefone. **Corrigido:** cada opção existe nas duas versões, e aparece a do modo atual.
> - Com isso, o policial e o Fixer **pessoalmente** também devem mostrar as opções. Antes elas eram só de telefone e o jogo as escondia.
> - No log, a abertura deve mostrar **"Loaded 37 custom dialogs"** (eram 25).

> **1.2.3:**
> - As missões do mod agora têm nome próprio com **[Hitman]** no final.
> - Menu Insert: janela **"Missões Hitman"** e seção/janela **"Mercados negros"**, com a senha de cada um. Detalhes na seção M (itens M7 a M9).

> **Resultado do 6º teste (1.2.1) e o que mudou na 1.2.2:**
> - ✅ O policial e o Fixer falam quando ligam para você, e o número vai para o caderno.
> - ✅ Tiros letais: todos mataram no 1º tiro (14 de 14 no log).
> - ❌ Ligar para o número deles não fazia nada (e dava erro vermelho). **Corrigido:** o jogo cria essa ligação sem telefone do outro lado, e o mod procurava o número no lugar errado; agora reconhece pela saudação.
> - ❌ O nome no caderno aparecia "<invalidstringaddress>". **Corrigido:** "Policial Corrupto" e "Fixer". Os números já salvos também são corrigidos ao carregar.
> - **Novo:** os 3 pacotes do policial são **depoimentos / gravações das câmeras / nome fora da investigação** (seção P).
> - **Novo:** falar **pessoalmente** com o policial e com o Fixer mostra as opções deles. Com o policial, você paga na hora.
> - ❌ NPCs comuns atirando em você. **Corrigido:** só Enforcers, cargos de briga, os assassinos do jogo e os caçadores usam arma de fogo. Os outros brigam na mão.
> - **Mira padrão 1,0** (era 0,9, e o desvio fazia o 1º tiro errar).
>
> **Repita:** P1, P3, P3b, P4, K2–K5 (também pessoalmente), L1–L6 e uma briga com um NPC comum.

> **Resultado do 5º teste (1.2.0) e o que mudou na 1.2.1:**
> - ✅ Entrega do contrato Distintivo validou, com pagamento e nota (C6, N5, V6).
> - ✅ Corpo achado: heat e cena de crime (V2). O sorteio da vingança rodou (T5).
> - ✅ A campanha começou (L1), mas a ligação foi muda.
> - ❌ Policial, Fixer e campanha mudos ao telefone. **Corrigido:** as ligações agora usam o mesmo tipo das ligações do próprio jogo.
> - ❌ Ligar para o policial ou o Fixer dava erro vermelho (`NullReferenceException`). **Corrigido:** o próprio NPC atende o número dele.
> - ❌ Interrogatório sem opções de resposta, e Enforcers entrando em casa. **Corrigido:** as respostas agora aparecem, e os Enforcers vêm para o corredor da sua porta.
> - ⚠️ Visto no log: o suborno nunca era aceito (limite Cr3), porque o salário do jogo vem em milhares. **Corrigido:** agora alvos com salário alto podem aceitar, e o pagamento varia de verdade com o cargo.
> - **Novo, como você pediu:** o número do policial e o do Fixer vão para o caderno na primeira ligação, atendida ou não.
>
> **Repita:** P1, P3, P4 (policial), I1–I4 (interrogatório), K2–K5 (Fixer), L1–L6 (campanha), S2 (suborno aceito).

> **1.2.0, campanha “O Último Contrato”** (seção **L**):
> - O Fixer liga com um cliente grande: 3 capítulos (contratos) que pagam mais.
> - O policial avisa que o Fixer te vendeu.
> - Você escolhe o final: matar o Fixer (dinheiro) ou entregá-lo ao policial (heat zerado).

> **1.1.0, consequências** (seção **T**):
> - **Cliente traidor**, às vezes, na entrega: fica com o resto do dinheiro (matá-lo cobra a dívida), chama a polícia ou manda um matador.
> - **Vingança:** alguém próximo da vítima contrata um matador.
> - **Alvo assustado:** se tranca em casa.
>
> O menu Insert tem a seção **Consequências (1.1)** para forçar cada uma.

> **1.0.0, catálogo de contratos** (seção **N**): quatro contratos novos de assassinato.
> - **Ligação:** o cliente diz tudo pelo telefone.
> - **Orelhão:** um orelhão toca na hora marcada.
> - **Passional:** o alvo trai o cliente.
> - **Distintivo:** o alvo é Enforcer, segurança ou cobrador.
>
> Cada um aparece num nível da carreira, e o menu Insert tem um botão "Postar: …" para cada um. Também corrigi os acentos quebrados no menu Insert.

> **0.9.0, carreira e Fixer** (seção **K**):
> - Reputação em níveis, com contratos liberados e pagamento maior.
> - O Fixer liga, dá contratos exclusivos e vende equipamento, incluindo o bilhete de suicídio.
> - Novo contrato **não letal** de intimidação, em paralelo.

> **0.8.0, arsenal e métodos** (seção **X**): armas de fogo do jogo (barulhentas), faca e katana, garrote por trás, veneno em quem está dormindo (morte "natural" horas depois) e suicídio forjado.

> **0.7.0, furtividade e evidências** (seção **V**):
> - O heat sobe quando o **corpo é achado** e quando uma **testemunha denuncia**, 1–2 h depois. Dá para subornar, adormecer ou calar a testemunha antes.
> - Só conta câmera **ligada**.
> - Cada entrega ganha uma **nota** (Assassino Silencioso / Profissional / Desleixado) com bônus.
> - O alvo subornado **some da cidade**.
> - O interrogatório ganha uma 3ª pergunta quando há evidência.

> **0.6.0:**
> - **Modo dev escondido**, como vai ser na versão pública. Na sua instalação eu já criei o arquivo `devmode.txt`, então as teclas de teste, o menu (Insert) e o log continuam ligados. As opções de dev foram para `BepInEx/config/sod.hitman.dev.cfg`.
> - **Sinal de 50%:** o cliente paga metade no encontro e o resto vem na entrega.

> **0.5.7: menu de teste.** Aperte **Insert** dentro do jogo. Ele reúne todas as teclas de teste e mais opções: teletransporte até o anúncio, o cliente, o alvo, o encontro, o policial e o orelhão; dinheiro, vida, invencível, invisível e modo fantasma; velocidade e salto de tempo; limpar procurado; e outras. Veja a seção **M**.

> **0.5.6:** a tranquilizante foi corrigida (seção **W**, item W3). Antes, o NPC dormia uns 5 dias e você ficava perseguido sem fim. Agora ele dorme **3 minutos reais**, e nem ele nem ninguém fica atrás de você.

> **Resultado do 4º teste (0.5.4):**
> - ✅ Missão da pizza até a morte do alvo, heat subindo nas mortes e F8 postando.
> - ❌ A entrega não validava (você fechou o caso): corrigido.
> - ❌ O policial ficava mudo: corrigido.
> - ⚠️ O F8 mostrava "?": corrigido.
>
> **Na 0.5.5 também mudou:** heat em 30/60/90, e o novo interrogatório no nível 2 (seção **I**). **Comece uma cidade nova.**
>
> **Resultado do 3º teste (0.5.3):** pistolas ✅ (W1–W3).

> ⚠️ **Não atualize o SOD.Common.** A sua versão do jogo (set/2024) só funciona com o **SOD.Common 2.0.7**. As versões 2.1.3 em diante quebram ao carregar e levam o Hitman junto.

> **Para a ordem de execução, siga o `ROTEIRO_DE_TESTE.md`.** Ele vai do menu principal até o fim em 3 sessões. Este checklist serve para anotar o resultado de cada código ([A1], [W3]…) e tem os detalhes de cada teste.

### Teclas de teste (só funcionam com a janela do jogo em foco)

| Tecla | O que faz |
|---|---|
| **F2** | Grava o dump (presets, cidadãos, jobs) |
| **F3** | Põe as duas pistolas no inventário |
| **F4** | O policial corrupto liga agora |
| **F8** | Posta mais um contrato (a vaga extra no quadro dura até o fim da sessão) |
| **F9** | Mostra na tela os contratos, o heat e o policial |
| **F10** | Mata o alvo do job aceito, como um tiro letal (dano + morte) |
| **F11** | Mata o alvo do job aceito (morte direta) |
| **Shift+F6 / Ctrl+F6** | Heat +10 / −10 |
| **F12** | Enforcers vêm te interrogar agora (fique em casa), em qualquer nível de heat |
| **Insert** | Abre e fecha o **menu de teste** (seção M) |
| Secundário / Primário (com pistola) | Mirar / Atirar |

O jogo já usa **F1, F5 e F7** no seu teclado, então o mod não usa essas teclas.

> **As pistolas agora fazem parte do mod.** **F3** põe as duas no inventário para testar: a **Lazarus 5 com silenciador** (letal) e a **Lazarus 5 tranquilizante**. Mire com o botão **secundário** e atire com o **primário**.

Marque cada item com ✅ (funcionou), ❌ (falhou) ou ⚠️ (estranho) e anote o que viu. Não precisa copiar nada para mim: eu leio o log e o dump direto do PC. Só tem uma regra importante:

> **Depois de fechar o jogo, não abra de novo antes de eu ler o log.** O `LogOutput.log` é apagado a cada vez que o jogo abre.

Tempo estimado: 2h–2h30 para tudo. Dá para dividir em sessões: **(1)** A, B, W e C; **(2)** S e H; **(3)** P e D. Se tiver pouco tempo, faça só **A, B, W4–W5, C1–C6, S1–S3, H1–H4, I1–I4 e P1–P4**.

> **Use um save só de teste.** As teclas de teste (F8, F10, Shift+F6…) mudam o save: contratos forçados, alvos mortos e heat ficam gravados.

---

## 0. Antes de abrir o jogo

- [ ] Em `E:\Shadows of Doubt\BepInEx\plugins\` existem estas pastas (eu já instalei tudo):
  - `Venomaus-SODCommon`
  - `Piepieonline-Asset_Bundle_Loader`
  - `Piepieonline-DDSLoader`
  - `Piepieonline-CommunityCaseLoader`
  - `SOD.Hitman` (o nosso mod: `SOD.Hitman.dll` + pastas `Contracts`, `Heat` e `Weapons`)
  - `SOD.InputSoftlockFix` (o seu mod anterior, não mexi)
- [ ] **Não** instale o `TranquilizerPistol` separado nem o mod **Guns**: os dois já estão dentro do Hitman (o Hitman se declara incompatível com o Guns).

## A. Carregamento (só abrir até o menu principal)

- [ ] **A1.** O jogo abre normalmente e chega ao menu.
- [ ] **A2.** Nada de tela preta, travamento ou erro visível.

(No log eu confiro: os 5 plugins carregados, `Loading manifest: HitmanPizza` do CommunityCaseLoader e nenhum erro de JSON.)

## B. Reconhecimento do jogo (Fase 0)

Comece um **jogo novo com cidade pequena** (ou carregue um save).

- [ ] **B1.** A cidade carrega normalmente. O mod grava sozinho um dump em `BepInEx\plugins\SOD.Hitman\dump\`.
- [ ] **B2.** Aperte **F8**. Aparece "[Dev] Contrato #N criado…" e logo depois "[Dev] Contrato #N postado em (local)", com o nome do lugar. ✅ na 0.5.4 (mas mostrava "?" no local, corrigido na 0.5.5).
  - ❌ Se aparecer "O jogo não conhece o job 'HitmanPizza'", o JSON não carregou. Pare aqui e me avise.
  - ⚠️ Se aparecer "O jogo não achou cliente e alvo…", aperte de novo. Anote se isso acontece sempre.
- [ ] **B3.** Aperte **F9**. Para cada contrato aparece na tela: onde está o anúncio, quem é o alvo, o cargo, o pagamento e se ele **aceitaria ou recusaria suborno**. Esse resumo é só para teste.
- [ ] **B4.** Pegue um **side job comum** (não o nosso) num quadro, aperte **F9**, depois **F10** (mata o alvo desse job). Espere ~1 minuto de jogo e aperte **F9** de novo. Anote se o job sumiu, falhou ou continuou.
- [ ] **B5. Nocaute vanilla.** Nocauteie um NPC qualquer (soco ou objeto) e espere ele acordar. Serve para confirmar no log que nocaute **não** dispara morte.

## W. Pistolas (novo na 0.5.0)

- [x] **W1. Pegar as pistolas.** ✅ (0.5.3) Aperte **F3**. As duas aparecem no inventário com os nomes "Lazarus 5 com silenciador" e "Lazarus 5 tranquilizante".
- [x] **W2. Mirar e atirar.** ✅ (0.5.3) Com uma delas na mão, segure o **secundário**: a câmera aproxima e os avisos mudam para "Mirar"/"Atirar". Aperte o **primário** para atirar.
  - Atire várias vezes seguidas: nunca acaba a munição e não há recarga, só um intervalo curto entre os tiros.
  - Sem mirar, o primário continua sendo uma pancada com a pistola (o normal do jogo).
- [ ] **W3. Dardo.** ❌ no teste da 0.5.4: ele dormia "para sempre" e você ficava procurado, com o ícone de olho na tela sem fim. Corrigido na 0.5.6: repetir. Atire com a **tranquilizante** num NPC qualquer.
  - Ele cai dormindo e **não morre**.
  - **Na hora do dardo:** nenhuma perseguição começa, você não fica procurado, e o ícone de olho não aparece (ou some logo).
  - **Acorda depois de uns 3 minutos reais** de jogo sem pausa (o pause não conta). Cronometre no celular.
  - Ao acordar, ele **não vem atrás de você**: volta à rotina como se nada tivesse acontecido.
  - Se ninguém viu, você não fica "ilegal".
  - Num save antigo com alguém dormindo há dias por causa de um dardo, essa pessoa acorda ao carregar.
- [ ] **W4. Tiro letal.** ❌ na 0.5.3 (o heat não subia), corrigido na 0.5.4: repetir. Num **save de teste**, atire com a **letal** num NPC que **não** é alvo de contrato, sem ninguém olhando.
  - Ele **morre** com um tiro.
  - Aperte **F9**: aparece "Mortes 1". **Desde a 0.7** o heat só sobe quando o corpo for achado (seção **V**).
  - Novo: o corpo deve ser um cadáver normal do jogo, que dá para examinar. Anote se aparece hora da morte ou algo parecido.
- [ ] **W5. Com testemunhas.** ❌ na 0.5.3, corrigido na 0.5.4: repetir. Mate outro NPC com a letal na frente de outras pessoas.
  - Você fica "ilegal" e as pessoas reagem. Desde a 0.7, cada testemunha denuncia 1–2 h depois (+15 cada) e a câmera só conta se estava ligada e te viu (seção **V**).
  - Anote se alguém acha o corpo e chama a polícia.
- [ ] **W6. Loja.** Vá a um **vendedor de armas** ou ao **mercado negro**. As duas pistolas estão à venda? Anote os preços.
- [ ] **W7. Salvar e carregar** depois de matar alguém: a vítima continua morta, como cadáver, e o heat não sobe de novo (o F9 mostra o mesmo valor de antes de salvar).
- [ ] **W8. (Opcional) Arma de crime.** Se achar uma Lazarus 5 com silenciador numa cena de crime, ela atira como a letal.

## C. Missão 1 — "Free Lancer para delivery de pizza"

Se o anúncio não aparecer em nenhum quadro, use **F8** de novo e **F9** para achar o local.

- [ ] **C1. Anúncio.** No quadro, um bilhete diz "CONTRATA-SE / Free Lancer para delivery de pizza / ¢valor / Ligue para (número)".
  - Anote o **valor**. Contratos devem pagar mais que um side job comum.
  - O texto está em português?
- [ ] **C2. Aceitar e ligar.** Aceite e ligue para o número de qualquer telefone. O cliente deve dizer algo como: "É sobre o delivery de pizza? Ótimo. Me encontre em (local) às (HH:MM). Vou estar com isto nas mãos: (item). Se não aparecer até (HH:MM), esqueça."
  - Aparece também uma mensagem na tela: "Encontro com o cliente às HH:MM. Tolerância até HH:MM."
  - ⚠️ Se no lugar da hora aparecer `??:??` ou texto estranho, anote e tire print.
- [ ] **C3. Encontro.** Vá ao local **antes do horário limite**. O cliente está lá com o item na mão. Fale com ele. Ele deve dizer: "Que bom que você veio. Preciso que (nome) durma um sono profundo e nunca mais me incomode. Essa praga vive tirando fotos dos pés da minha família na rua. Chega. Trabalha em (empresa). O resto é com você. Entregue por baixo da porta em (endereço)."
  - Aparece a mensagem "Detalhes do serviço atualizados".
  - **Novo (0.6):** aparece "O cliente adiantou Cr… O resto, Cr…, vem na entrega." e o dinheiro entra na carteira (metade do valor do anúncio).
  - No quadro do caso, o alvo aparece com nome e local de trabalho?
  - Anote se o cliente **te entregou uma maleta/dossiê** (o encontro vanilla às vezes entrega).
- [ ] **C4. Investigação.** Ache o alvo (local de trabalho → ficha de funcionários etc.). Funcionou como um caso normal?
- [ ] **C5. Matar.** ✅ na 0.5.4. Mate o alvo com a **pistola letal**. Se a arma não funcionar, use **F10** perto dele (ou **F11** se o F10 não matar).
  - Aparece "Contrato cumprido. Preencha o formulário de resolução e entregue o caso."
  - O caso **continua aberto** (não foi cancelado) e o objetivo "Mate (nome)" ficou completo.
  - Novo: no formulário do caso, a pergunta sobre o alvo aparece como **cumprida**.
- [ ] **C6. Entrega.** ❌ na 0.5.4 (a pergunta "matar" não era marcada, então a entrega não validava), corrigido na 0.5.5: repetir. Preencha o formulário de resolução e passe por baixo da porta do cliente.
  - Você recebeu o dinheiro? Desde a 0.6, a entrega paga o **resto**: valor do anúncio menos o sinal do encontro (C3).
  - ⚠️ **Não use "fechar caso"**: isso encerra o job sem pagamento.
- [ ] **C7. Atraso** (outro contrato: F8 de novo). Aceite, ligue, **não vá** ao encontro e espere passar o horário limite (dormir ou esperar).
  - Aparece "Você não apareceu no encontro. O cliente cancelou o contrato." e o caso falha.
- [ ] **C8. Desmaiar não conta.** Num contrato ativo, atire no alvo com a **tranquilizante** (ou nocauteie com soco).
  - O contrato **não** deve ser concluído.
- [ ] **C9. Salvar e carregar.** Com um contrato aceito e o encontro marcado: salve, carregue e aperte **F9**. Depois de matar o alvo: salve, carregue e faça a entrega.
  - Nada se perdeu (horário do encontro, contrato cumprido, pagamento)?
- [ ] **C10. (Opcional) Cliente morto.** Mate o cliente de um contrato ativo. O contrato deve ser encerrado.

## S. Suborno (novo na 0.2.0)

Use contratos diferentes para cada resultado. Poste vários com **F8**, e o **F9** mostra qual alvo "aceitaria" e qual "recusaria". A opção só aparece **depois do encontro** com o cliente.

- [ ] **S1. Pessoa errada.** Com um contrato ativo, converse com alguém que **não** é o alvo. A opção "Alguém me pagou Cr(valor) pra te apagar. Me paga Cr(dobro) e eu esqueço que você existe." aparece.
  - Escolha a opção. A pessoa diz "Do que você tá falando? Me deixa em paz." e nada mais acontece. O contrato continua.
- [ ] **S2. Aceita** (alvo que o F9 marca como "aceitaria"). Fale com o alvo e escolha a opção.
  - Ele diz "...Tá bom. Toma o dinheiro. Você nunca me viu."
  - Você recebe **Cr(dobro do contrato)** e aparece "Suborno aceito: Cr… O contrato foi encerrado."
  - O caso some ou fecha sem entrega, e o alvo continua vivo.
- [ ] **S3. Recusa e foge** (alvo "recusaria" com cargo comum).
  - Ele diz "Socorro! Alguém chame a polícia!" e foge. Anote se a polícia aparece ou se dispara algum alarme.
  - O contrato continua: dá para matar o alvo e entregar normalmente.
- [ ] **S4. Recusa e luta** (alvo "recusaria" com cargo de luta: Enforcer, segurança, cobrador, vendedor de armas, dono de cassino, assassino de aluguel). Esse alvo é mais raro; teste se aparecer.
  - Ele diz "Você escolheu a pessoa errada pra ameaçar." e te ataca.
- [ ] **S5. Uma tentativa só.** Depois de uma recusa, a opção não aparece mais para aquele contrato.
- [ ] **S6. Salvar e carregar** depois de uma recusa: a opção continua sem aparecer.

## H. Heat (novo na 0.3.0)

Teclas de teste: **Shift+F6** soma 10 de heat e **Ctrl+F6** tira 10. Cada uso mostra na tela "[Dev] Heat X pts (nível N)…". O **F9** também mostra o heat.

- [ ] **H1. Heat de contrato.** Depois do C5, aperte **F9**. Desde a 0.7 o heat sobe **10** (+5 em lugar público, +10 com câmera que te viu) **quando o corpo for achado**, e +15 por testemunha que denunciar.
> **Novos limiares na 0.5.5: 30 / 60 / 90** (antes 50/100/160). O mod atualiza o seu `.cfg` sozinho, se você não tiver mudado esses valores.

- [ ] **H2. Nível 1** (chegue a 30 com Shift+F6). Aparece "Os jornais estão falando de um matador de aluguel na cidade. (Heat nível 1)".
  - **No dia seguinte do jogo**, pegue um jornal (banca, lanchonete, mesa). A primeira página deve ser "MATADOR DE ALUGUEL À SOLTA?". Tire um print se puder.
- [ ] **H3. Nível 2** (60). Aparece "Os Enforcers estão investigando as mortes… (Heat nível 2)".
  - Em até 1 hora de jogo começa o interrogatório (seção **I**). A ronda com falas de rádio não acontece mais no nível 2.
  - No jornal do dia seguinte: "ENFORCERS FECHAM O CERCO AO MATADOR".
- [ ] **H4. Nível 3** (90). Aparece "Há um mandado contra você…".
  - Entre em qualquer prédio. Os Enforcers e seguranças te tratam como procurado (atacam ou perseguem)?
  - Em até 2 horas de jogo aparece "Você sente que está sendo seguido." Alguém armado vem atrás de você?
  - No jornal do dia seguinte: "MANDADO EXPEDIDO CONTRA MATADOR DE ALUGUEL".
- [ ] **H5. Descer de nível.** Use **Ctrl+F6** até ficar abaixo de 90. Aparece "Heat reduzido para o nível 2". O caçador para de te seguir? O mandado acaba depois de um tempo?
- [ ] **H6. Salvar e carregar** com heat acima de 0. O F9 mostra o mesmo valor depois de carregar.

## I. Interrogatório (novo na 0.5.5)

No nível 2, a visita dos Enforcers virou um interrogatório **na sua casa**, sem revista. Tecla de teste: **F12** faz a visita acontecer agora, em qualquer nível (fique em casa).

Como funciona:
- **Quando:** a visita espera você estar em casa. Um Enforcer chega perto de você e faz **2 perguntas**: onde você estava na hora da última morte, e se você conhecia a última vítima.
- **Credibilidade:** cada resposta é crível, neutra ou suspeita conforme o que a polícia poderia saber.
  - "Em casa, dormindo" é mentira se a última morte teve testemunha ou câmera.
  - "Nunca ouvi falar" é mentira se a vítima morava no seu prédio.
  - "Não é da sua conta" e "Quero um advogado" sempre pesam contra.
- **Resultado:** convenceu, heat −10. Neutro, nada muda. Desconfiou, +10 ou +20. No fim, os Enforcers vão embora.

- [ ] **I1. Chegada.** Em casa, com heat 60+ (ou F12). Em até 1 hora de jogo, um Enforcer chega perto de você e fala "Polícia. Estamos investigando umas mortes no bairro… Onde você estava (dia), por volta das (hora)?". A mesma frase aparece na tela, junto com "Fale com o Enforcer para responder."
  - Anote se ele bateu na porta, se entrou e quantos Enforcers vieram.
- [ ] **I2. Pergunta 1.** Fale com ele. Aparecem as 3 respostas da pergunta 1. Escolha uma.
  - Ele reage e faz a pergunta 2: "Você conhecia (nome)?". O nome é o da sua última vítima?
- [ ] **I3. Pergunta 2.** As 3 respostas da pergunta 2 aparecem, sozinhas ou falando com ele de novo. Escolha uma.
- [ ] **I4. Resultado.** Ele fecha com uma frase final, aparece o resultado com o heat, e os Enforcers **vão embora sem revistar** a casa. Confira o heat no F9.
- [ ] **I5. Sair no meio.** Com o Enforcer falando, saia do prédio. Aparece "Você saiu no meio do interrogatório. (heat +10)".
- [ ] **I6. Nunca em casa.** Com heat entre 60 e 89, fique fora de casa por mais de 1 dia de jogo. Aparece "Os Enforcers passaram no seu apartamento várias vezes e nunca te encontraram… (heat +10)".
- [ ] **I7. Não responder.** Deixe o Enforcer esperando sem falar com ele por uns 45 minutos de jogo. Aparece "O Enforcer cansou de esperar pelas suas respostas. (heat +10)".

## P. Policial corrupto (novo na 0.4.0)

Tecla de teste: **F4** faz o policial ligar na hora. O **F9** mostra o nome dele, o número, se o contato já foi feito e o preço do ponto.

- [ ] **P1. Primeira ligação.** ❌ na 0.5.4 (ele ficava mudo por causa de um erro do SOD.Common com diálogos no telefone), corrigido na 0.5.5: repetir. Fique **em casa** e suba o heat até 60 (Shift+F6). Em uns 15 minutos de jogo o telefone do apartamento toca. Se não quiser esperar, use **F4**.
  - Atenda. **Ele fala** pelo telefone algo como "Aqui é (nome), da delegacia… Papel some, sabe? Por um preço. Anota aí: (número)."
  - O número aparece no seu caderno como **"Policial Corrupto"**? (Na 1.2.1 aparecia "<invalidstringaddress>"; corrigido na 1.2.2.)
  - Nas próximas vezes que ele liga, ele só lembra o número ("…me liga no (número) ou me procura"). A compra é feita ligando para ele.
- [ ] **P2. Não atender.** Num save sem contato feito (ou depois de um policial novo assumir), use **F4** **longe de casa, perto de um orelhão**.
  - Primeiro toca em casa (você não ouve). Quando parar, deve tocar um **orelhão perto de você** (até 40 m).
  - Se não atender nenhum, ele tenta de novo 6 horas de jogo depois.
- [ ] **P3. Comprar pelo telefone.** (Na 1.2.1 ligar para ele não fazia nada; corrigido na 1.2.2.) Ligue para o número dele. Ele atende "Fala. O que você quer que suma do arquivo?" e aparecem **3 pacotes**, do mais barato ao mais caro. Cada um mostra o heat que tira e o preço:

  | Pacote | O que faz |
  |---|---|
  | 1. "Some com os depoimentos das testemunhas" | Os depoimentos que já foram à polícia somem (o heat deles sai), e as testemunhas que ainda não falaram não falam mais |
  | 2. "Apaga as gravações das câmeras e os depoimentos" | O mesmo do 1, mais as gravações de câmera. Corpo já achado perde o heat da câmera; corpo escondido nunca vai trazer esse heat |
  | 3. "Tira meu nome da investigação" | Heat a zero e nada do que está no arquivo traz heat de novo (inclusive corpos ainda não achados). Cancela uma visita de interrogatório pendente |

  - Sem testemunha nem câmera contra você, os pacotes 1 e 2 mostram "(nada para apagar)".
  - Escolha um. Ele responde "Fechado. Deixa Cr… num envelope no orelhão em (local). Você tem até (hora)." e aparece a mensagem com o local.
- [ ] **P3b. Comprar pessoalmente.** Use "Ir ao policial" no menu e fale com ele cara a cara. Os mesmos 3 pacotes aparecem.
  - O pagamento é na hora, sem envelope. Aparece a mensagem do efeito (ex.: "Os depoimentos contra você sumiram… (heat −N)").
  - Sem dinheiro: "Isso custa Cr…. Volta quando tiver."
- [ ] **P4. Envelope.** Vá até o orelhão com o dinheiro e fique bem perto dele (até 3 m).
  - Aparece "Você deixou o envelope…", o dinheiro sai da carteira e o heat cai (F9).
  - Sem dinheiro suficiente, aparece "Você não tem Cr… para o envelope."
- [ ] **P5. Preço dobrado.** Com heat 90+ (mandado), os preços das opções devem ser o **dobro**.
- [ ] **P6. Prazo.** Escolha um pacote e não vá ao orelhão. Depois de 24 horas de jogo aparece "O policial cansou de esperar…".
- [ ] **P7. (Opcional) Policial morto.** Descubra quem ele é (o nome está no F9), mate-o e espere 3 dias de jogo. Um novo policial liga dizendo que assumiu.
- [ ] **P8. Salvar e carregar** depois do contato. O número continua funcionando e o F9 mostra o mesmo policial.

## V. Furtividade e evidências (novo na 0.7)

Agora o heat vem do que a polícia **descobre**, e não só de "você matou":
- **Corpo:** a morte só conta quando alguém acha o corpo. Vale +10 num contrato e +20 nas outras mortes, +5 em lugar público e +10 se uma câmera ligada te viu. Corpo escondido não gera heat nem jornal. Corpo achado vira **cena de crime**: os Enforcers vão ao local.
- **Testemunhas:** cada uma vai à polícia **1–2 h de jogo depois** e vale +15. Antes disso você pode:
  - **subornar**: fale com ela, Cr200, aceita em 75% das vezes; Enforcers nunca aceitam; quem recusa denuncia na hora e foge;
  - **adormecer com o dardo**: ela esquece o que viu;
  - **matar**: aí é outra morte, com o seu próprio risco.
- **Câmeras:** só contam se estavam **ligadas** e te viram. Desligar a segurança ou o disjuntor antes vira estratégia.
- **Nota do serviço** na entrega:

  | Nota | Condições | Bônus |
  |---|---|---|
  | **Assassino Silencioso** | só o alvo morreu desde o aceite, nenhuma testemunha falou, sem câmera, corpo não achado | +25% |
  | **Profissional** | só o alvo morreu e nenhuma testemunha falou | +10% |
  | **Desleixado** | qualquer outro caso | — |

  A nota também dá **reputação** (para a carreira, na 0.9).
- **Suborno 2.0:** quem aceita o suborno **some da cidade**. O contrato conta como feito, então você ainda recebe do cliente na entrega. Fora da missão da pizza, o cliente pode descobrir (25% de chance, 1 a 3 dias depois) e mandar alguém atrás de você.
- **Interrogatório 2.0:** se uma testemunha já falou ou uma câmera te pegou, o Enforcer faz uma **3ª pergunta**: "Temos (imagens de câmera / uma testemunha) que te colocam em (local). O que você fazia lá?"

O menu Insert tem uma seção **Evidências**: ver o último crime, testemunhas pendentes, testemunhas denunciam já, silenciar testemunhas, achar o corpo escondido.

- [ ] **V1. Sem heat na hora.** Mate alguém sem ninguém ver. O F9 mostra "Mortes 1", heat sem mudança e "corpos não achados 1".
- [ ] **V2. Corpo achado.** Quando alguém acha o corpo (ou com "Achar corpo escondido" no menu), aparece "O corpo de (nome) foi encontrado. (heat +N)". Os Enforcers vão ao local?
- [ ] **V3. Testemunha denuncia.** Mate alguém na frente de outra pessoa. Aparece "(nome) viu tudo…". Depois de 1–2 h: "(nome) contou à polícia o que viu. (heat +15)".
- [ ] **V4. Suborno da testemunha.** Antes de ela denunciar, fale com ela: a opção "Você não viu nada, certo? Toma Cr200…" aparece. Aceitou? O dinheiro saiu e ela não denunciou? Recusou? Denunciou na hora e fugiu?
- [ ] **V5. Dardo na testemunha.** Acerte uma testemunha pendente com o dardo. Aparece "(nome) vai acordar sem lembrar do que viu" e ela não denuncia.
- [ ] **V6. Nota.** Na entrega de um contrato, aparece "Avaliação do serviço: …" com o bônus. Faça um limpo (Silencioso) e um com testemunha (Desleixado), se der.
- [ ] **V7. Câmera.** Mate alguém num lugar com câmera ligada e veja se aparece "Uma câmera registrou você". Depois desligue a segurança do prédio (painel ou disjuntor) e repita: a mensagem não deve aparecer.
- [ ] **V8. Suborno 2.0.** Num alvo que aceita o suborno: ele some da cidade, o contrato aparece como cumprido e a entrega paga o cliente também.
- [ ] **V9. Interrogatório com 3ª pergunta.** Com uma testemunha que já denunciou, use F12 em casa. Depois das 2 perguntas, vem a 3ª sobre o local.

## X. Arsenal e métodos (novo na 0.8)

Os itens são todos do próprio jogo, sem modelo nem animação nova. O menu Insert, em **Armas → "Dar um item do arsenal"**, entrega um de cada vez.

| Método | Itens | Como funciona | Custo |
|---|---|---|---|
| **Armas de fogo barulhentas** | Revólver, espingarda, rifle de sniper, rifle de batalha | Miram e atiram como a letal; os rifles com mais zoom | **Barulho**: todos a até 30 m viram testemunhas, e o tiro é ilegal para quem vê. À venda nas lojas de armas |
| **Faca, katana, estilete** | Facas de cozinha, faca de combate, katana | Dano ×2; o golpe que derruba (ou o 2º golpe) mata | Mata na frente de quem estiver olhando |
| **Garrote** | Corda (`RopeWeapon`), arame | **Por trás** (ou em alguém caído), mata de uma vez, sem grito | Precisa chegar por trás; de frente é só um golpe normal |
| **Veneno** | Seringa, frasco de veneno | Em alguém **dormindo ou nocauteado** (combina com o dardo!), a pessoa morre **1–3 h de jogo depois**, em outro lugar | Morte **"natural"**: só +2 de heat quando acham o corpo, sem cena de crime, sem testemunha |
| **Suicídio forjado** | Bilhete de suicídio na casa da vítima | Veneno ou garrote com o bilhete na casa = "suicídio" | 0 de heat. O bilhete vai ser vendido pelo Fixer (0.9) |

- [ ] **X1. Arma barulhenta.** Pegue o revólver no menu. Mire e atire em alguém. Aparece "(nome) viu tudo…" também para quem estava perto **sem ver**? Os NPCs reagem ao som?
- [ ] **X2. Sniper.** Pegue o rifle de sniper. A mira aproxima bem mais que a pistola?
- [ ] **X3. Faca.** Com uma faca na mão, ataque um NPC. Ele morre no golpe que o derruba (ou no 2º)?
- [ ] **X4. Garrote.** Com a corda, chegue **por trás** de alguém e ataque. Ele morre na hora, sem grito? De frente é só um golpe?
- [ ] **X5. Veneno.** Adormeça alguém com o dardo e use a seringa nele. Aparece "Você envenenou (nome)…". Ele acorda, segue a vida e morre 1–3 h depois. Quando acham o corpo: "Parece morte natural. (heat +2)".
  - Atalho no menu: "Alvo morre de veneno já" faz o alvo do contrato morrer de veneno na hora. O contrato conta?
- [ ] **X6. Lojas.** O vendedor de armas e o mercado negro vendem o revólver, a espingarda e os rifles?

## K. Carreira, Fixer e intimidação (novo na 0.9)

- **Reputação:** vem das notas.

  | Situação | Reputação |
  |---|---|
  | Assassino Silencioso | +3 |
  | Profissional | +2 |
  | Desleixado | +1 |
  | Suborno ou intimidação | +1 |

- **Níveis:**

  | Nível | A partir de | Pagamento |
  |---|---|---|
  | Iniciante | 0 | ×1 |
  | Profissional | 5 | ×1,25 |
  | Veterano | 15 | ×1,5 |
  | Lenda | 30 | ×2 |

  Cada contrato precisa de um nível para aparecer nos quadros. Os da 1.0 vão do nível 1 ao 3.
- **Intimidação (não letal, em paralelo):** um bilhete "PROCURA-SE alguém para entregar um recado pessoalmente".
  - O cliente pede uma **surra, nada de matar**.
  - Paga cerca de 40% de um assassinato, sempre aparece e não é obrigatória.
  - O jogo confere a surra pelo próprio objetivo de "dar uma surra".
- **O Fixer:** um cidadão com emprego criminoso (dono de casa de apostas, contador do crime, mercado negro…).
  - Ele liga para a sua casa quando você tem **1 ponto de reputação** (depois do primeiro serviço entregue) e deixa um número.
  - Ligue para ele e escolha:
    - **"Tem serviço pra mim?"**: ele posta um contrato exclusivo do melhor nível que você tem, pagando +20%, e avisa em que quadro. Um a cada 12 h.
    - **"Como anda a minha fama?"**: a sua ficha (nível, reputação, serviços, mortes, ganhos, heat).
    - **"Quero comprar: …"**: seringa, frasco de veneno, **bilhete de suicídio**, corda, rifle de sniper. O item vai direto para o inventário.
  - Se ele morrer, outro assume 3 dias depois.

O menu Insert tem a seção **Carreira e Fixer**: +5 de reputação, ficha, Fixer liga agora, número no caderno, ir ao Fixer. O botão de postar a intimidação foi para a seção **Contratos** ("Postar: Intimidação").

- [ ] **K1. Reputação.** Entregue um contrato. Aparece a nota e, ao passar de nível, "Sua reputação subiu: agora você é…". Use "+5 de reputação" no menu para pular.
- [ ] **K2. Ligação do Fixer.** Com 1+ de reputação, em casa: o telefone toca e ele se apresenta e dita o número (ou use "Fixer liga agora"). O número vai para o caderno como "Fixer"?
- [ ] **K3. Serviço do Fixer.** Ligue para ele e escolha "Tem serviço pra mim?". Ele responde e logo aparece "O Fixer deixou o serviço num quadro: (lugar)". O contrato paga mais que um da pizza?
- [ ] **K4. Loja.** Ligue e compre algo. O dinheiro sai e o item chega ao inventário? Sem dinheiro: "Volta quando tiver o dinheiro."
- [ ] **K5. Ficha.** "Como anda a minha fama?" mostra nível, reputação, serviços e ganhos.
- [ ] **K6. Intimidação.** Use "Postar: Intimidação" (ou espere aparecer um bilhete "PROCURA-SE…"). Aceite, ligue, encontre o cliente, dê a surra no alvo **sem matar** e entregue. Pagou cerca de 40% de um assassinato?
- [ ] **K7. Suicídio forjado com a loja.** Compre o bilhete de suicídio, deixe-o na casa do alvo (largue o item lá dentro) e mate-o com veneno ou garrote. Quando acharem o corpo: "Parece suicídio. (heat +0)".

## N. Catálogo de contratos (novo na 1.0)

Quatro contratos novos de assassinato. Todos usam o mesmo motor da pizza: só a morte do alvo conta, sinal de 50%, nota na entrega, suborno, entrega por baixo da porta do cliente.

| Contrato | Bilhete no quadro | Como chega a informação | Nível | Botão no menu |
|---|---|---|---|---|
| **Ligação** (`HitmanCall`) | "PRECISA-SE / Resolvedor de problemas, discreto" | Você liga e o cliente já diz o alvo, o motivo e onde ele trabalha | Profissional (5) | Postar: Ligação |
| **Orelhão** (`HitmanPayphone`) | "TRABALHO RÁPIDO / Só para quem sabe ouvir" | Você liga e o cliente marca um orelhão e uma hora. Lá, o orelhão toca e alguém dita o alvo | Profissional (5) | Postar: Orelhão |
| **Passional** (`HitmanPassion`) | "PRECISA-SE / Alguém que entenda de coração partido" | Encontro marcado, como na pizza. O alvo trai o cliente e pode morar com ele | Veterano (15) | Postar: Passional |
| **Distintivo** (`HitmanBadge`) | "SERVIÇO DE ALTO RISCO / Paga-se muito bem" | Encontro marcado. O alvo é Enforcer, capitão, segurança ou cobrador | Lenda (30) | Postar: Distintivo |

Os botões "Postar: …" funcionam mesmo acima do seu nível. Para testar os níveis de verdade, use "+5 de reputação".

- [ ] **N1. Ligação.** Poste, aceite e ligue para o número do bilhete.
  - Na mesma ligação, o cliente diz o alvo, o motivo, "Trabalha em …" e a entrega?
  - Aparecem "Detalhes do serviço atualizados" e o sinal de 50%?
- [ ] **N2. Orelhão.** Poste, aceite e ligue. O cliente marca um orelhão e uma hora (o texto é do próprio jogo). Vá até lá e atenda na hora.
  - A voz dita o alvo?
  - Aparecem o aviso de detalhes e o sinal?
  - O jogo deixou uma maleta com dossiê no orelhão? (Anote, porque o jogo pode deixar.)
- [ ] **N3. Passional.** Poste, aceite, ligue e vá ao encontro.
  - O alvo é o parceiro ou o amante do cliente?
  - Se morarem juntos, a entrega continua por baixo da porta da casa do cliente.
- [ ] **N4. Distintivo.** Poste, aceite, ligue e vá ao encontro.
  - O alvo é Enforcer, segurança ou cobrador?
  - Ele reage armado?
  - Paga mais que a pizza?
- [ ] **N5. Entrega.** Faça um dos novos até o fim: mate o alvo, preencha o caso e entregue.
  - O caso validou?
  - Apareceram a nota e o bônus?
- [ ] **N6. Níveis.** Com a carreira ligada e reputação 0, só pizza e intimidação aparecem sozinhos nos quadros.
  - Com 5+: Ligação e Orelhão.
  - Com 15+: Passional.
  - Com 30+: Distintivo.

  Isso demora, porque o jogo só repõe os quadros de tempos em tempos.
- [ ] **N7. Fixer.** Num nível alto, "Tem serviço pra mim?" posta o contrato do maior nível que você tem?

> Se um "Postar: …" responder "the game found no client/target pair that fits the motive", anote qual contrato foi. Significa que a cidade não tem um par cliente/alvo que sirva, e isso é mais provável no Passional e no Distintivo.

## T. Consequências (novo na 1.1)

As reviravoltas só vêm de **clientes e alvos**. O policial continua confiável, e o heat só cai pelo policial e pelo interrogatório.

| Consequência | Quando | O que acontece |
|---|---|---|
| **Cliente não paga** | Na entrega (10% de chance, nunca no 1º serviço) | O resto do dinheiro não vem e não há bônus da nota. Se o cliente morrer pelas suas mãos, você recebe o que ele devia e +1 de reputação |
| **Cliente chama a polícia** | Na entrega | Ele paga, mas chama os Enforcers para onde você está (heat +15) |
| **Cliente manda um matador** | Na entrega, sem aviso | 12–36 h depois: "(cliente) não quer pontas soltas…", e um matador vem atrás de você |
| **Vingança** | Quando acham o corpo de uma vítima (15%, ou 30% se a polícia tem câmera ou testemunha) | Alguém próximo da vítima (parceiro, colega de casa, amigo) contrata um matador, que chega 12–36 h depois. Morte "natural" ou suicídio não geram vingança |
| **Alvo assustado** | O alvo recusa o suborno e foge, ou vê você matar alguém | Ele se tranca em casa por 12 h de jogo |

Botões em **Insert → Consequências (1.1)**: "Próxima entrega: não paga / polícia / caçador", "Assustar o alvo atual", "Vingança agora", "Dívidas e caçadores". O status no topo do menu mostra a linha **Consequências**.

- [ ] **T1. Cliente não paga.** Clique "Próxima entrega: não paga" e entregue um contrato.
  - Aparece "(cliente) ficou com o resto do seu dinheiro (CrX) e sumiu…"?
  - O dinheiro não subiu (só o sinal ficou)?
- [ ] **T2. Cobrar a dívida.** Depois do T1, use "Ir ao cliente" e mate-o.
  - Aparece "Dívida cobrada: … CrX são seus" e o dinheiro sobe?
- [ ] **T3. Polícia.** Clique "Próxima entrega: polícia" e entregue.
  - Aparece "(cliente) pagou, mas chamou a polícia…" com heat +15?
  - Os Enforcers vêm até você?
- [ ] **T4. Matador do cliente.** Clique "Próxima entrega: caçador" e entregue. Nada aparece na hora.
  - Avance o relógio 24–36 h: aparece "(cliente) não quer pontas soltas…" e "Você sente que está sendo seguido."?
  - Alguém vem atrás de você?
- [ ] **T5. Vingança.** Clique "Vingança agora".
  - No minuto seguinte de jogo aparece a mensagem e um matador vem atrás de você?
  - No jogo normal, isso pode acontecer depois que acham um corpo: anote se viu.
- [ ] **T6. Alvo assustado.** Com um contrato aceito, clique "Assustar o alvo atual" (ou ofereça suborno a um alvo que foge).
  - Aparece "(alvo) ficou com medo e vai se trancar em casa…"?
  - Use "Ir ao alvo" um pouco depois: ele foi para casa e fica lá?
- [ ] **T7. Save.** Com uma dívida ou um matador a caminho (veja em "Dívidas e caçadores"), salve, saia e carregue.
  - A dívida e o matador continuam lá?

## L. Campanha “O Último Contrato” (novo na 1.2)

| Etapa | O que acontece |
|---|---|
| **Início** | Com reputação 5 (Profissional) e o Fixer já conhecido, ele liga para a sua casa: um cliente grande, três serviços. Atenda em casa; se não atender, ele tenta de novo 6 h depois |
| **Capítulos 1–3** | Ligue para o Fixer e escolha **"Sobre o cliente grande…"**. Ele posta o contrato do capítulo e avisa o quadro. Os capítulos são: 1 Ligação ("o teste"), 2 Orelhão ("a testemunha"), 3 Distintivo ("o uniforme"). Pagam ×1,5, além dos +20% do Fixer. Depois dos capítulos 1 e 2, o Fixer liga de novo |
| **Traição** | Depois do capítulo 3, o **policial** liga: o Fixer te vendeu, e um matador já está a caminho |
| **Final A** | Mate o Fixer: Cr5.000, +5 de reputação, os matadores desistem |
| **Final B** | Ligue para o policial e escolha **"Quero entregar (Fixer)"**: o Fixer some (preso), o heat vai a zero, +3 de reputação |
| **Falhas** | Se o contrato de um capítulo falhar, peça de novo ao Fixer. Se o Fixer morrer antes do fim, a campanha acaba sem final. Depois do fim, outro Fixer assume, como sempre |

Botões em **Insert → Campanha (1.2)**: "Começar (o Fixer liga)", "Pular capítulo", "Traição agora (o policial liga)". A linha **Campanha** no topo do menu mostra a etapa.

- [ ] **L1. Início.** Em casa, clique "Começar (o Fixer liga)".
  - O telefone toca e o Fixer fala do cliente grande?
  - Aparece "Nova história: “O Último Contrato”…"?
- [ ] **L2. Capítulo 1.** Ligue para o Fixer.
  - Aparece a opção "Sobre o cliente grande…"?
  - Escolha. Ele explica o serviço e aparece "“O Último Contrato”, capítulo 1: o bilhete está num quadro em …"?
- [ ] **L3. Entrega do capítulo.** Faça o contrato até o fim.
  - Aparece "capítulo 1 concluído. (Fixer) vai ligar…"?
  - Ele liga em ~1 h de jogo com o próximo?
- [ ] **L4. Capítulos 2 e 3.** Repita, ou use "Pular capítulo" para ir mais rápido. O pagamento dos capítulos é maior que o normal?
- [ ] **L5. Traição.** Depois do capítulo 3 (ou com "Traição agora"), em casa:
  - O policial liga, dizendo que o Fixer te vendeu?
  - Aparecem "… te vendeu. Mate (Fixer), ou ligue para (policial)…" e "Você sente que está sendo seguido."?
- [ ] **L6. Final B.** Ligue para o policial.
  - Aparece "Quero entregar (Fixer)."?
  - Escolha. Aparece o final com "Heat zerado", e o heat vai a 0?
- [ ] **L7. Final A** (outro save, ou repita a partir do L5). Use "Ir ao Fixer" e mate-o.
  - Aparecem o final com Cr5.000 e o dinheiro?
- [ ] **L8. Save.** No meio da campanha, salve e carregue. A etapa continua a mesma na linha "Campanha" do menu?

## M. Menu de teste (novo na 0.5.7)

Aperte **Insert** para abrir e fechar. Com o menu aberto, o mouse fica livre e a câmera e o movimento ficam parados, para você clicar nos botões. As pistolas não atiram. A janela pode ser arrastada pela barra do título e redimensionada pelo canto.

O topo mostra, ao vivo: hora e velocidade do tempo, dinheiro, heat, interrogatório, o contrato atual (alvo, cliente e valor), o policial e o envelope, e quantos NPCs estão dormindo por dardo.

| Seção | Opções |
|---|---|
| **Contratos** | Postar contrato, listar contratos, um "Postar: …" para cada contrato (1.0), ir ao anúncio, ir ao ponto de encontro, ir ao cliente, ir ao alvo, matar o alvo (tiro ou direto), marcar cumprido com o alvo vivo, ver perguntas do caso |
| **Heat** | +10, −10, zerar, ir direto ao nível 1, 2 ou 3, interrogatório agora, cancelar interrogatório, Enforcers em casa (reação normal do jogo), dispensar caçadores |
| **Policial corrupto** | Ligar agora, pôr o número no caderno, ir ao policial, ir ao orelhão do envelope, pagar o envelope já |
| **Armas** | Dar as duas pistolas, acordar os tranquilizados, todos os NPCs esquecem você |
| **Jogador** | Vida cheia, +Cr1.000, +Cr10.000, limpar procurado, ir para casa, onde estou. Liga/desliga: invencível, invisível, silencioso, sem dano de queda, modo fantasma (atravessa paredes), sem invasão de propriedade |
| **Cidade e NPCs** | Liga/desliga: pausar IA, NPCs não reagem a ataques, chaves da cidade (todas as portas) |
| **Tempo** | Velocidade normal, rápida ou muito rápida; adiantar o relógio 1 h, 6 h ou 24 h |
| **Ferramentas** | Gravar dump, fechar o menu |

> As opções liga/desliga do jogador e da cidade são chaves de debug do próprio jogo. Algumas podem não ter efeito nesta versão, e é justamente isso que o item M4 serve para descobrir.

- [ ] **M1. Abrir.** Com a cidade carregada, aperte **Insert**. O menu aparece à direita, com o mouse livre e o status no topo.
  - Se aparecer "O menu ainda está carregando", espere alguns segundos e tente de novo.
- [ ] **M2. Fechar.** Aperte **Insert** de novo (ou o X da janela). O mouse volta a controlar a câmera e você anda normalmente.
  - ⚠️ Se a câmera ou o movimento ficarem travados depois de fechar, anote o que você fez antes.
- [ ] **M3. Teletransporte.** Com um contrato aceito, teste "Ir ao alvo", "Ir ao cliente" e "Ir ao anúncio". Você aparece perto de cada um?
- [ ] **M4. Liga/desliga.** Ligue "Invisível" e passe na frente de alguém; ligue "Modo fantasma" e atravesse uma parede; ligue "Invencível" e leve um soco. Anote quais funcionaram.
- [ ] **M5. Tempo.** "+1 h" adianta o relógio? "Muito rápido" acelera o tempo?
- [ ] **M6. Contrato sem matar.** Num contrato aceito, use "Marcar cumprido (alvo vivo)" e faça a entrega. Serve para testar o pagamento sem caçar o alvo.
- [ ] **M7. Nome [Hitman].** Aceite um contrato do mod. O caso aparece com o nome da missão e **[Hitman]** no final (ex.: "Entrega de pizza [Hitman]")?
- [ ] **M8. Janela "Missões Hitman".** Clique "» Missões Hitman" (seção Contratos). Abre uma janela à esquerda com cada missão ativa.
  - Cada missão mostra: número, nome, se está no quadro ou aceita, alvo e cargo, cliente, pagamento e sinal, se o alvo aceitaria suborno, onde está o bilhete, o tipo de chegada (encontro, orelhão, ligação) e, depois de aceita, o encontro, os detalhes recebidos e a morte.
  - Teste os botões: "Ir ao bilhete", "Aceitar já" (abre o caso como o botão do bilhete), "Ir ao cliente", "Ir ao alvo", "Ir ao encontro", "Matar o alvo", "Marcar cumprido", "Encerrar".
  - "Atualizar" redesenha a lista; "Fechar" fecha a janela.
- [ ] **M9. Mercados negros (os 3 tipos).** Na seção "Mercados negros", clique "» Lista dos três tipos". Abre uma janela com cada **mercado negro**, **clínica clandestina** (sync disks) e **cassino ilegal**, do mais perto ao mais longe. Também entra qualquer outro lugar que o jogo tranque com senha.
  - Cada um mostra o tipo, o nome, a **senha**, onde fica (prédio e andar) e quem atende, com o aviso "é o seu Fixer" quando for ele.
  - "Ir para a porta (lado de fora)" te põe no corredor ou na rua da entrada. A senha bate com a da porta?
  - "Ir a quem atende"; "Aprender a senha" (o jogo passa a considerar que você sabe).
  - Apareceram os três tipos? Anote se algum tipo de negócio clandestino ficou de fora.
  - Na seção também há "Ir ao mais próximo (porta)" e "Aprender todas as senhas".

## D. Regressão

- [ ] **D1.** Side jobs comuns continuam aparecendo nos quadros.
- [ ] **D2.** Complete um side job comum até o fim. O pagamento veio normal?

## E. Se algo der errado (sem precisar de mim)

Arquivo de configuração: `E:\Shadows of Doubt\BepInEx\config\sod.hitman.cfg` (criado na primeira vez que o jogo abre com o mod).

| Problema | O que mudar |
|---|---|
| O contrato falha mesmo depois de falar com o cliente | `[Contract] EnforceMeetingDeadline = false` |
| Quer mais ou menos tempo até o encontro | `[Contract] MeetDelayMinutes` (padrão 60) |
| Alguma tecla de teste conflita com o jogo | `[Dev] KeyMenu` (menu, Insert), `KeyDump`, `KeyGiveWeapons`, `KeyCopCall`, `KeyForcePost`, `KeyListJobs`, `KeyKillTarget`, `KeyMurderTarget`, `KeyHeatUp`, `KeyHeatDown`, `KeyInterrogate` |
| Valores de pagamento estranhos | `[Payment] Base`, `SalaryDivisor`, `Min`, `Max` (e me avise) |
| Sinal no encontro maior, menor ou nenhum | `[Payment] DepositShare` (0.5 = metade; 0 = tudo na entrega) |
| Ligar/desligar o modo dev | Criar/apagar `devmode.txt` em `BepInEx\plugins\SOD.Hitman\` (as opções ficam em `sod.hitman.dev.cfg`) |
| Voltar ao heat "na hora" (sem corpo e testemunhas) | `[Stealth] Enabled = false` |
| Níveis de carreira e pagamento por nível | `[Career] TierThresholds` (0,5,15,30), `TierPayMultipliers` (1,1.25,1.5,2) |
| Nível que cada contrato exige | `[Career] ContractTiers` (contrato:nível) |
| Sem carreira (tudo liberado desde o início) | `[Career] Enabled = false` |
| Intimidação paga pouco ou muito | `[Career] NonLethalPayMultiplier` (0.4) |
| O que o Fixer vende e por quanto | `[Fixer] Shop` (item:preço, até 5) |
| Fixer liga cedo/tarde, dá serviço demais/de menos | `[Fixer] FirstContactReputation` (1), `JobCooldownHours` (12), `ContractBonus` (0.2) |
| Quais itens contam como arma de fogo, faca, garrote ou veneno | `[Arsenal] LoudFirearms`, `Blades`, `Garrotes`, `Poisons` (nomes de item, separados por vírgula) |
| Tiro barulhento alcança muita ou pouca gente | `[Arsenal] HearingRadius` (30 m) |
| Faca forte ou fraca demais | `[Arsenal] BladeDamageMultiplier` (2), `BladeHitsToKill` (2) |
| Veneno demora pouco ou muito | `[Arsenal] PoisonDelayMinHours` / `PoisonDelayMaxHours` (1–3 h) |
| Heat da morte natural ou do suicídio | `[Arsenal] NaturalDeathHeat` (2), `ForgedSuicideHeat` (0) |
| Testemunhas demoram pouco ou muito | `[Stealth] WitnessDelayMinHours` / `WitnessDelayMaxHours` (1–2 h) |
| Suborno de testemunha caro, barato ou fácil demais | `[Stealth] WitnessBribePrice` (200), `WitnessBribeAcceptChance` (0.75) |
| Dardo não deve calar testemunha | `[Stealth] DartSilencesWitness = false` |
| Bônus das notas | `[Stealth] RatingBonusSilentAssassin` (0.25), `RatingBonusProfessional` (0.1) |
| Sem cena de crime | `[Stealth] CrimeScene = false` |
| Cliente trai demais, de menos ou nunca | `[Consequences] TraitorClientChance` (0.1; 0 = nunca) |
| Vingança frequente ou rara, matador cedo ou tarde | `[Consequences] RevengeChance` (0.15), `RevengeDelayMinHours` / `RevengeDelayMaxHours` (12–36 h) |
| Alvo assustado fica pouco ou muito em casa | `[Consequences] ScaredTargetHours` (12) |
| Desligar todas as consequências | `[Consequences] Enabled = false` |
| Campanha começa cedo ou tarde | `[Campaign] StartReputation` (5) |
| Capítulos pagam pouco ou muito, final A paga pouco ou muito | `[Campaign] PayMultiplier` (1.5), `KillFixerReward` (5000) |
| Desligar a campanha | `[Campaign] Enabled = false` |
| Alvo subornado não some / sem risco do cliente | `[Bribe] TargetLeavesTown = false`, `ClientDiscoveryChance = 0` |
| Ninguém aceita suborno, ou todos aceitam | `[Bribe] SalaryShare`, `SalaryPeriods` (e me avise) |
| A opção de suborno aparece com todo mundo e incomoda | `[Bribe] ShowToEveryone = false` (passa a aparecer só com o alvo) |
| Desligar o suborno | `[Bribe] Enabled = false` |
| Algum efeito do heat atrapalha o jogo | `[Heat] Newspaper`, `Investigations`, `Warrant`, `Hunters` (cada um pode ser desligado) |
| Desligar o heat inteiro | `[Heat] Enabled = false` |
| Níveis de heat curtos ou longos demais | `[Heat] Level1`, `Level2`, `Level3` (padrão 30/60/90) |
| Interrogatório atrapalha ou quer a ronda antiga | `[Interrogation] Enabled = false` |
| Interrogatório pesa demais ou de menos | `[Interrogation] HeatWhenConvinced`, `HeatPerSuspiciousPoint`, `HeatWhenAvoided` |
| O Enforcer desiste rápido ou demora para chegar | `[Interrogation] WaitMinutes` (45), `ArrivalTimeoutHours` (3), `AvoidHours` (24) |
| O policial liga em hora ruim / insiste demais | `[CorruptCop] RetryHours` (padrão 6) |
| Preços do policial caros ou baratos demais | `[CorruptCop] FallbackPricePerPoint`, `FilePremium` (0.25, extra do pacote 3), `WarrantMultiplier` |
| Quer NPCs comuns brigando com arma de fogo de novo | `[Arsenal] CiviliansUseGuns = true` |
| O envelope não é deixado ao chegar no orelhão | `[CorruptCop] DropRadius` (padrão 3 m) |
| Contrato nunca aparece nos quadros | Use F8; se nem assim, me mande o log |
| A letal precisa de mais de um tiro, ou mata fácil demais | `[Lethal] ShotsToKill` (padrão 1), `DamageMultiplier` |
| O dardo faz dormir pouco ou demais | `[Tranquilizer] SleepDurationSeconds` (padrão 180 = 3 minutos reais) |
| Mira imprecisa, recuo ou zoom incomodam | `[Weapons] Accuracy`, `RecoilAmplitude`, `ZoomInOnAimPct` |
| Quer inverter (vanilla = tranquilizante) | `[Weapons] TranquilizerPreset` e `LethalPreset` (troque os nomes) |
| Não quer as pistolas à venda | `[Weapons] SellAtShops` vazio |

## F. O que eu verifico no log depois (só para referência)

- Plugins e manifest carregados, sem erro de JSON, DDS ou Harmony.
- `[Payment]`: a conta do pagamento, incluindo se a vítima aceitaria suborno com a regra do spec.
- `[Meet]`: hora do encontro, variáveis de texto, quando a informação foi recebida.
- `[Kill]` e `[Guard]`: morte registrada e bloqueio do cancelamento automático do jogo.
- `[Probe]`: o que o jogo fez com os jobs (respostas para as perguntas do spec marcadas **[VERIFICAR NO CÓDIGO]**).
- Dump: nomes reais dos intros, hand-ins e diálogos, salários por cargo.

## Pontos que só o teste vai confirmar

1. Se o jogo aceita os JSON da missão (clonados de `VandalismHome_D3`).
2. Se a variável de hora no texto (`|sidejob.custom_hitman_meettime|`) funciona. Se não funcionar, a mensagem na tela continua mostrando a hora.
3. Se o jogo avisa "informação recebida" na conversa do encontro. É disso que depende a regra dos 15 minutos.
4. Como o jogo reage quando o alvo morre (o mod bloqueia o cancelamento).
5. A unidade do salário (anual?), que define se os valores de pagamento fazem sentido.
6. Se o encontro também entrega uma maleta, além da conversa.
7. Se a opção de suborno aparece no diálogo (é registrada pelo SOD.Common).
8. Se a resposta de recusa faz o alvo realmente lutar ou fugir. Se não lutar, o mod força uma perseguição.
9. Heat: ~~se a morte causada pelo jogador chega ao mod~~ (na 0.5.3 não chegava; causa achada e corrigida na 0.5.4) e se a contagem de testemunhas funciona.
10. Heat: se trocar o artigo principal do jornal funciona, se o "procurado no prédio" do jogo dura o bastante e se o caçador consegue atravessar a cidade atrás de você.
11. Policial: se a ligação criada pelo mod toca de verdade e se a fala dele aparece ao atender.
12. Policial: se as opções de pacote aparecem quando você liga para o número dele.
13. ~~Pistolas: se o código de tiro (herdado do Guns) funciona nesta versão do jogo.~~ ✅ Funcionou no 3º teste.
14. Pistolas: se o item novo da tranquilizante carrega, aparece com o nome certo e é vendido nas lojas.
15. ~~Pistolas: em que unidade o jogo conta a duração do sono forçado.~~ Em horas de jogo (lido no código). O mod agora conta o sono em tempo real.
16. Interrogatório: se o Enforcer que veio na chamada fica perto de você até a conversa, se as respostas da pergunta 2 aparecem na mesma conversa e se marcar a chamada como "concluída" faz os Enforcers irem embora.
17. Policial: se a fala dele pelo telefone (árvore DDS, como a ligação do LEM no capítulo 1) aparece na tela.
18. Menu: se a interface do UniverseLib abre bem neste jogo e se a câmera e o movimento voltam ao fechar.
19. Catálogo: se o intro de orelhão do jogo (`Dossier-TelephoneGooseChase`) toca o orelhão e usa o nosso diálogo para ditar o alvo.
20. Catálogo: se a cidade tem pares para o Passional (alvo com amante secreto, como no motivo vanilla `Lust`) e para o Distintivo (alvo com cargo de luta).
21. Catálogo: se, nos contratos sem encontro, o sinal vem na ligação (Ligação) ou no orelhão (Orelhão).

---

## Histórico de atualizações

### 1.2.6 — 20/10/2026 (Fixer reformulado)
- **Telefone:** a saudação do Fixer (`endsDialog`) diz onde ele está (`WhereNow`: casa, `employer.placeOfBusiness` ou o local atual). O menu dele passou a ser só cara a cara, a até 8 m (`FaceToFace`).
- **Serviço na hora:** `Fixer.HandOver(job)` roda quando o bilhete sobe (`Postings`):
  - `SideJob.AcceptJob()` abre o caso e chama `SetHandIn`;
  - `SideJob.OnAcquireJobInfo(dialog de info do intro)` aplica pistas e fatos e dispara o sinal (`MeetingClock`).

  A fala vem de `Fixer.Brief(job)`. A campanha usa o mesmo caminho.

### 1.2.5 — 20/10/2026 (correção do 8º teste)
- Lido em `InteractionController.RefreshDialogOptions`:
  - Remoto: `GetDialogOptions(DataKey.voice)`.
  - Cara a cara: `photo` + `voice`.
  - Ordenação por `DialogPreset.ranking`, decrescente.
- O SOD.Common cria os presets com `tiedToKey = photo` e `ranking = 1`. `PhoneDialogs.MenuOption` põe `voice` e `ranking 1000` nas opções do policial, do Fixer e no "entregar"; interrogatório, suborno e testemunha ganharam só o ranking.

### 1.2.4 — 20/10/2026 (correção do 7º teste)
- **Opções de telefone sumindo:** as opções do policial, do Fixer e o "entregar o Fixer" agora são registradas duas vezes, uma com `telephoneCallOption` verdadeiro e outra com falso.
  - `IsDialogShown` mostra só a forma que bate com `InteractionController.isRemote` (`PhoneCalls.DialogIsRemote`).
  - No 7º teste, a conversa começava remota e um instante depois virava cara a cara com o NPC, o que escondia as opções de telefone.
- **Fixer:** as opções herdam de `FixerOption` (forma, rótulo próprio, visibilidade).
- **Policial:** envelope sempre que ele não está a menos de 8 m de você (`CopFar`); perto, pagamento na hora.
- **Menu Insert:** "Ir à porta da casa do Fixer / do policial (lado de fora)" (`DevActions.TeleportOutsideHome`, mesma porta usada para os mercados negros).
- Ainda a investigar: um erro em `DialogController.IdentifyNumber` (o serviço de identificar a última chamada) logo depois de uma ligação da campanha.

### 1.2.3 — 20/10/2026
- **Nome das missões:** cada `JobPreset` usa o próprio nome como `caseName`, que o `AcceptJob` traduz por `missions.postings`. As strings ganharam "… [Hitman]". `hitmancontract` (casos de saves antigos) virou "Contrato [Hitman]".
  - ⚠️ O gerador `gen_catalog.ps1` copia o JSON da pizza: se for rodado de novo, corrigir o `caseName`.
- **`Dev/DevWindows.cs` (novo):** `ListPanel` (painel UniverseLib com Atualizar/Fechar e lista redesenhada), `HitmanJobsPanel`, `BlackMarketsPanel` e `BlackMarkets`.
  - **Mercados negros:** endereços com `AddressPreset` `BlackmarketTrader`/`BlackmarketSyncClinic`. A senha vem de `NewAddress.GetPassword()`, "aprender" é `GameplayController.SetPlayerKnowsPassword`, e a porta é o nó do outro lado de `entrances`.
- **Ações por contrato em `DevActions`:** `AcceptNow` (= `SideJob.AcceptJob`, o mesmo do botão do bilhete), `KillTarget(job)`, `EndJob(job)` e os teleportes.

### 1.2.2 — 20/10/2026 (correções do 6º teste)
- **Número falso:** `KeypadController` cria a ligação com `to = null` e passa a `CallSource` registrada. O `PhoneCalls` reconhece quem atende pela saudação dessa fonte (`SetAnswerer(greeting)`), e `InCopCall`/`InFixerCall` passam a olhar `receiver` e a saudação.
- **Nome no caderno:** o `textOverride` de `AddOrMergePhoneNumberData` é uma chave das strings `evidence.generic`. Foram criadas as chaves `hitman_corruptcop` e `hitman_fixer` em `content/Heat`, mais os textos antigos como chaves.
- **Pacotes do policial:** `KillEvidence.WitnessValue/CameraValue/FutureValue` e `EraseWitnesses/EraseCameras/EraseAll`. O preço é por ponto tirado agora ou evitado; o pacote 3 custa `FilePremium` (25%) a mais. O antigo `AllDiscount` saiu. O envelope guarda o pacote (`PendingKind`); saves antigos usam `PendingPoints`.
- **Cara a cara:** as opções do policial e do Fixer aparecem quando `saysTo` é ele e o contato já foi feito. Com o policial o pagamento é na hora.
- **`Weapons/CivilianGuns.cs`:** prefix em `NewAIController.SetCurrentWeapon`. Arma de fogo só para Enforcer, `FighterJobs`, assassino do jogo (`MurderController`) ou caçador; os outros recebem `SetCurrentWeapon(null)` e brigam com os punhos.
- `[Weapons] Accuracy` padrão 1,0; 0,9 é migrado.

### 1.2.1 — 20/10/2026 (correções do 5º teste)
- **`PhoneCalls.cs` (novo).** Lido no código do jogo:
  - Toda ligação que o jogo faz para o jogador (oferta do assassino, orelhão dos side jobs) é `CallSource(CallType.player, saudação)`, sem quem liga e com receptor específico. O mod usava `CallType.dds`, em que quem liga fala a árvore DDS onde está, longe do jogador.
  - A saudação nova é `HitmanPhoneLine` (`content/Phone`), com o bloco da fala trocado antes de cada ligação.
- **Número falso sem quem atenda:** o `ExecuteDialog` do jogo não aguenta isso (`NullReferenceException`). Um prefix em `PhoneCall.SetCallState` põe o policial ou o Fixer como quem atende (`recevierNS`), como o cliente de um side job faz.
- **`defaultOption = true`** nas opções de telefone e nas respostas do interrogatório. O SOD.Common 2.0.7 só oferece diálogos que estão em `Toolbox.defaultDialogOptions`; com `false`, eles nunca apareciam.
- **Interrogatório:** `CallEnforcers` vai para o corredor do outro lado da porta (`home.entrances`). Antes ia para o apartamento, e a equipe do jogo entrava para revistar.
- **Salário:** `Payment.AnnualSalary` = `salary × 1000`. O jogo guarda o salário em milhares (100 aparece como Cr100000).
- Números do policial e do Fixer vão para o caderno quando a ligação é criada.

### 1.2.0 — 20/10/2026 (roadmap: campanha)
- **`Contracts/StoryCall.cs` (novo):** qualquer cidadão liga para o apartamento do jogador e fala uma frase. Usa a mesma árvore DDS das ligações do policial e do Fixer, e avisa quando o jogador atende ou perde a ligação.
- **`Contracts/Campaign.cs` (novo):** etapas 0–5 salvas em `HeatState` (`CampaignStage`, `CampaignJobId`, `CampaignCallAt`, `CampaignBetrayed`, `CampaignEnding`).
  - Os capítulos usam `Postings.Request` com requester "campaign", e o pagamento ganha `CampaignPayMultiplier` em `Career.PayMultiplier`.
  - O fim do capítulo vem de um postfix em `SideJob.Complete`.
  - Nova opção no telefone do Fixer ("Sobre o cliente grande…") e no do policial ("Quero entregar…").
  - No final "entregue", o Fixer sai com `RemoveFromWorld` (fica em `LeftTownIds`), e o substituto liga depois de `FixerReplaceDays`.
- Os contratos da campanha não sofrem traição do cliente (`Consequences`).
- `HeatSystem.CancelPersonalHunters`: os matadores desistem no final.

### 1.1.0 — 20/10/2026 (roadmap: consequências)
- **`Contracts/Consequences.cs` (novo):**
  - **Cliente traidor.** O sorteio é feito num prefix de `Case.Resolve`, antes do pagamento (o jogo paga a "case reward" ali dentro).
    - "Não paga": zera a pergunta do contrato e devolve o que o jogo tiver pago mesmo assim. Cria uma dívida (`Grudge`), que é cobrada quando o cliente morre pelas mãos do jogador; essa morte conta heat de contrato.
    - "Polícia": `CallEnforcers` onde o jogador está, heat +15.
    - "Caçador": um matador pessoal agendado.
  - **Vingança:** em `KillEvidence.BodyFound`, sorteia; o vingador é o parceiro, ou o amigo, colega de casa ou amante mais próximo (`Acquaintance.like`).
  - **Alvo assustado:** objetivo vanilla `RoutineControls.layLow`, criado como o jogo faz com o escritor de bilhetes do capítulo (sem duração, com a casa como local). Sai com `NewAIGoal.Remove()` no fim do prazo.
  - Tudo é salvo em `HeatState` (`PendingHunters`, `Grudges`, `Scared`).
- **`Rating`:** sem bônus de nota quando o cliente não paga.
- **Menu Insert:** seção "Consequências (1.1)" e uma linha de status.

### 1.0.0 — 20/10/2026 (roadmap: catálogo de contratos)
- **Quatro contratos novos** em `content/Contracts/`. Todos copiam `VandalismHome_D3`, como a pizza, cada um com intro, motivo, diálogos e textos PT/EN próprios:
  - `HitmanCall`: intro `Direct-TelephoneCall-D0`, motivo `Money`.
  - `HitmanPayphone`: `Dossier-TelephoneGooseChase`, com a 1ª ligação vanilla `Job_Chase_Start`.
  - `HitmanPassion`: encontro, motivo `Lust` (amante secreto; pode morar com o cliente).
  - `HitmanBadge`: encontro, motivo `RevengeGeneral` com Enforcers e só cargos Enforcer, EnforcerCaptain, SecurityGuard e DebtCollector.
- **Objetivo próprio por contrato:** cada um tem o seu `HitmanKill<Nome>`. O `HitmanKill` original mora na pasta da pizza, e a ordem de carga entre pastas não é garantida. `ContractRegistry` reconhece qualquer objetivo que comece com `HitmanKill`.
- **Sinal sem encontro:** antes, o `MeetingClock` ignorava todo diálogo terminado em "CallDialog" (é a ligação que marca o encontro). Agora isso vale só para contratos com encontro. Nos outros, conta o nosso diálogo e se ignora a ligação vanilla (`Job_*`).
- **`[Career] ContractTiers`:** pizza 0, intimidação 0, ligação 1, orelhão 1, passional 2, distintivo 3. O padrão da 0.9.0, que tinha nomes provisórios, é migrado.
- **Menu Insert:** um botão "Postar: …" por contrato. Também consertei o `DevMenu.cs`, que estava com os acentos em dupla codificação: o menu da 0.9.0 mostrava "intimidaÃ§Ã£o".
- README reescrito (EN + PT) com todas as funções. Versão 1.0.0 nos pacotes Thunderstore e Nexus.

### 0.9.0 — 20/10/2026 (roadmap, pilar 3: carreira e Fixer)
- **`Contracts/Career.cs`:** reputação e níveis.
  - O nível libera contratos: `Postings` zera a curva de frequência dos contratos acima do seu nível.
  - O pagamento multiplica por nível, por contrato não letal e pelo bônus do Fixer.
  - A ficha do assassino guarda serviços, notas Silencioso e ganhos.
- **`Contracts/Postings.cs`:** a vaga extra no quadro saiu do modo dev e virou um módulo público. O F8 e o Fixer usam o mesmo código.
- **`Contracts/Fixer.cs`:** intermediário com emprego criminoso.
  - Liga com a árvore DDS do policial e registra um número falso.
  - No telefone: serviço exclusivo, ficha, loja com 5 itens.
- **Intimidação** (`content/Contracts/HitmanIntimidate`): contrato não letal com o objetivo vanilla `PurpBeatUp`. A lógica de morte, o suborno e as proteções valem só para contratos de morte (`ContractRegistry.IsKillContract`).

### 0.8.0 — 20/10/2026 (roadmap, pilar 2: arsenal e métodos)
- **`Weapons/Arsenal.cs`:**
  - As armas de fogo vanilla atiram como a letal, mas são barulhentas: quem ouve vira testemunha, o tiro é ilegal e tem clarão. Os rifles têm zoom maior.
  - Facas: dano ×2, matam no golpe que derruba ou no 2º golpe.
  - Garrote: mata de uma vez, por trás ou em alguém caído.
  - Veneno em quem está dormindo ou nocauteado: morte natural 1–3 h depois (`WeaponKills.DieNaturally`).
  - Suicídio forjado: bilhete na casa da vítima.
- **`KillEvidence`:** guarda o método da morte. Morte natural ou suicídio gera pouco ou nenhum heat e não gera cena de crime.
- **Lojas:** vendem as armas de fogo extras.
- **Menu Insert:** "Dar um item do arsenal" e "Alvo morre de veneno já".

### 0.7.0 — 20/10/2026 (roadmap, pilar 1: furtividade e evidências)
- **Heat pelo que a polícia descobre** (`Heat/KillEvidence.cs`):
  - O corpo conta quando é achado (patch em `Death.SetReported`) e vira cena de crime.
  - As testemunhas denunciam 1–2 h depois (lista por morte, no save).
  - A câmera só conta se estava ligada e te viu (`SecuritySystem.isActive` + alvo rastreado ou mesmo cômodo).
- **Silenciar testemunhas:** suborno por diálogo (`Heat/WitnessTalk.cs`), dardo ou morte.
- **Nota do serviço** na entrega (`Contracts/Rating.cs`, patch em `SideJob.Complete`), com bônus e reputação.
- **Suborno 2.0:** o alvo some da cidade (`RemoveFromWorld`) e o cliente paga também. Fora da missão 1, o cliente pode descobrir e mandar um caçador (`HeatSystem.SendPersonalHunter`).
- **Interrogatório:** 3ª pergunta quando há evidência; "Em casa, dormindo" é mentira quando uma testemunha falou ou uma câmera te pegou.
- **Menu Insert:** nova seção Evidências; o status mostra evidências e reputação.

### 0.6.0 — 20/10/2026
- **Backup da 0.5.7 estável** em `Desktop\SoD Hitman Mod BACKUPS\v0.5.7-estavel-2026-10-20` (com LEIA-ME de como voltar).
- **Modo dev escondido** (decidido 20/10/2026, para a versão pública):
  - Sem o `devmode.txt` na pasta do plugin (ou sem `DevMode = true` no cfg), não há teclas de teste, menu nem sondas.
  - As opções de dev ficam num arquivo próprio, `sod.hitman.dev.cfg`.
  - Na sua instalação o `devmode.txt` já existe.
- **Sinal:** o cliente adianta metade no encontro (`[Payment] DepositShare = 0.5`) e a entrega paga o resto.
- **Pacotes:** Thunderstore e Nexus (com `INSTALL.txt`); README novo em inglês e português.

### 0.5.7 — 19/10/2026
- **Menu de teste dentro do jogo** (tecla **Insert**, configurável em `[Dev] KeyMenu`). Seção **M**.
  - Feito com o UniverseLib, que o SOD.Common já instala e inicializa. Nenhuma dependência nova.
  - Com o menu aberto, a câmera, o movimento e as pistolas ficam parados.
  - Ele reúne todas as teclas de teste e acrescenta: teletransportes, dinheiro, vida, limpar procurado, velocidade e salto de tempo, as chaves de debug do próprio jogo (invencível, invisível, silencioso, pausar IA…), modo fantasma, acordar os tranquilizados, fazer todos esquecerem você, marcar contrato cumprido sem matar e ver as perguntas do caso.

### 0.5.6 — 19/10/2026
- **Tranquilizante** (relato do seu teste: o NPC dormia sem fim, você ficava procurado e o ícone de olho não saía da tela).
  - **Causa 1 (sono eterno):** a duração ia direto para o `SetKO` do jogo, que conta em **horas de jogo**. Os "120" eram 120 horas, uns 5 dias.
  - **Causa 2 (perseguição sem fim):** mesmo com dano zero, a reação do jogo a um tiro do jogador faz o NPC te perseguir (`SetPersue`). Dormindo, ele continuava na sua lista de perseguidores, e é essa lista que mantém o ícone de olho.
  - **Correção:** o dardo não passa mais pela reação de dano do jogo. O mod põe o NPC para dormir por **3 minutos reais** de jogo sem pausa (decidido 19/10/2026).
  - **Limpeza:** ao dormir e ao acordar, o mod apaga você da "memória" do NPC (perseguição, combate, investigação, alvos rastreados, o que ele viu de ilegal) e o tira da sua lista de perseguidores e testemunhas.
  - Se um save for carregado no meio do sono, o relógio do próprio jogo acorda o NPC num tempo equivalente.
  - Quem estava dormindo há dias num save antigo acorda ao carregar.
  - O `.cfg` antigo (`SleepDurationSeconds = 120`) passa para 180.

### 0.5.5 — 19/10/2026
- **Quarto teste:** a missão da pizza funcionou até a morte do alvo, o heat subiu nas mortes e o F8 postou. Corrigido o que falhou:
  - **Entrega do contrato.** Ao aceitar um job, o jogo **move** as perguntas de resolução do job para o caso e esvazia a lista do job. O mod procurava a pergunta "matar" só no job, então, depois do aceite, a morte nunca marcava a pergunta, e a entrega não validava (por isso você fechou o caso). Agora o mod procura no caso.
  - **Policial mudo.**
    - O SOD.Common 2.0.7 converte quem recebe um diálogo em `Citizen` sem checar o tipo. No telefone esse lado é você ou o aparelho, então a conversão quebrava e o jogo cancelava o diálogo. O mesmo erro quebrava as opções de pacote. O Hitman agora decide esses diálogos antes do SOD.Common.
    - A ligação do policial para você passou a usar uma árvore de conversa DDS, como a ligação do LEM no capítulo 1. É o formato do jogo para quem liga falar pelo telefone.
  - **"?" no F8.** O job sai da criação sem anúncio. O F8 agora avisa "criado" e mostra o local quando o anúncio vai para o quadro.
- **Heat mais curto:** níveis em **30 / 60 / 90**. O `.cfg` antigo é atualizado sozinho, se você não tinha mudado esses valores.
- **Novo: interrogatório no nível 2** (seção **I**). Decidido 19/10/2026: só interrogatório, 2 perguntas, sem revista.
  - O Enforcer vem quando você está em casa e pergunta onde você estava na hora da última morte e se conhecia a vítima.
  - As respostas valem pelo que a polícia poderia saber: testemunha ou câmera, vítima vizinha.
  - Resultado: −10, 0, +10 ou +20 de heat. Depois a chamada é encerrada.
  - Fugir, não estar em casa por 1 dia ou não responder: +10.
  - No nível 3 (mandado) continua a resposta normal do jogo.
- Tecla de teste nova: **F12** (interrogatório agora).
- Para diagnóstico: o log agora registra quem encerrou um contrato aceito (pilha de chamadas do jogo). A ferramenta `tools/disasm` ganhou busca de chamadas virtuais.

### 0.5.4 — 19/10/2026
- **Terceiro teste:** as pistolas funcionaram ✅, mas o heat não subia e o F8 não postava contrato.
- **Heat que não subia.**
  - **Causa:** nesta versão do jogo, o registro de morte (`Human.Death`) exige um objeto de caso de assassinato. Sem ele, o `Human.Murder` quebrava no meio, com o erro `NullReferenceException at Human+Death..ctor` repetido no log a cada morte.
  - **O que isso causava:** a vítima ficava marcada como morta, mas faltava todo o resto: hora e local da morte, assassino, corpo, lista de mortos e o código do Hitman que conta o heat e conclui o contrato.
  - **Correção:** o mod passa um caso "em branco" que não inicia nenhum caso de assassinato. Confirmei no código do jogo que os três lugares que leem esse caso aceitam vazio. Por garantia, o heat e o contrato também são avisados direto pela pistola.
  - O F10 e o F11 tinham o mesmo problema e foram corrigidos junto.
  - A morte "do próprio jogo" pelo dano (`enableKill`) também quebra quando o assassino é você, então a pistola letal não a usa mais: quem mata é sempre o mod.
- **Mortes ao carregar o save.** Com o registro de morte, o save do jogo guarda o corpo sozinho. Ao carregar, o jogo "mata de novo" cada corpo salvo, e o mod agora ignora isso: o heat não sobe ao carregar.
- **F8 que não postava.** O jogo recalcula as vagas de cada tipo de job a cada verificação, então o valor que o mod mudava era apagado na hora. Agora o F8 abre uma vaga extra por esse cálculo do próprio jogo. Ela dura até o fim da sessão, porque, se sobrasse contrato, o jogo tiraria um do quadro.
- **Descoberta:** o jogo só encerra por "pessoa morta ou assassino" os jobs **ainda não aceitos**. Era o caso do contrato #23 do seu teste, que sumiu quando o alvo virou o assassino do caso da cidade. A proteção do Hitman agora só vale para contrato aceito, e o anúncio de um alvo já morto sai do quadro.
- **Nova ferramenta `tools/disasm`:** desmonta métodos do jogo e lista quem chama cada um. Foi assim que achei a causa.

### 0.5.3 — 19/10/2026
- **Segundo teste:** o jogo fechava sozinho antes de abrir.
  - **Causa:** dois patches do Hitman estavam em métodos que o IL2CPP **funde** com milhares de outros de corpo idêntico: `SideJob.OnGooseChaseCallTriggered` (vazio, compartilhado com 4.245 métodos) e `RevengeObjective.ManualTrigger` (compartilhado com 386, incluindo renderização). O patch rodava para todos eles e o Rewired caiu dentro dele ao iniciar.
  - **Correção:** os dois foram removidos, e a conclusão do contrato pela morte não depende deles.
- **Nova ferramenta `tools/foldcheck`:** lê o código nativo do jogo e diz se um método foi fundido.
  - Os 23 patches do Hitman e os 47 dos outros mods foram verificados, e todos são únicos.
  - O `pack.ps1` agora se recusa a empacotar se algum patch cair num método fundido.

### 0.5.2 — 19/10/2026
- **Primeiro teste:** o SOD.Common 2.1.5 falhou ao carregar (`Could not load type 'ModSource'`), porque ele mexe no menu de mods de uma versão mais nova do jogo. O BepInEx então pulou o Hitman, e por isso nenhuma tecla funcionava.
- **Correção:** instalado o **SOD.Common 2.0.7**, da mesma época do seu jogo.
  - Verificado com uma ferramenta nova (`tools/refcheck`): os 6 plugins instalados usam só tipos e membros que existem no seu jogo, com 0 faltando. Antes, o SOD.Common 2.1.5 tinha 11.
  - O Hitman passou a salvar os dados por save com uma função que existe em todas as versões do SOD.Common.
- Corrigido o aviso do DDSLoader sobre a tree do anúncio.

### 0.5.1 — 19/10/2026
- Verificação de compatibilidade da pasta do jogo antes do primeiro teste:
  - Só 6 plugins instalados, sem Guns, TranquilizerPistol avulso ou LethalAction.
  - Dependências nas versões certas e cópias do UniverseLib idênticas.
  - Arquivos instalados iguais aos do projeto.
  - Interop gerado depois da última atualização do jogo.
  - Log `Info` ligado no BepInEx.
- Teclas de teste trocadas para não colidir com o jogo, que usa F1, F5 e F7 no seu teclado: dump F7 → **F2**, pistolas Shift+F7 → **F3**, policial Shift+F5 → **F4**.

### 0.5.0 — 19/10/2026
- O seu **TranquilizerPistol foi incorporado ao Hitman**, e agora há duas pistolas silenciadas:
  - **Letal:** a Lazarus 5 com silenciador do jogo (a dos assassinos). Mata com um tiro (configurável).
  - **Tranquilizante:** um item novo, "Lazarus 5 tranquilizante". Faz dormir e nunca mata.
- Munição infinita e **sem recarga**: o jogo não tem sistema nem animação de recarga.
- As pistolas são obtidas **só no mundo ou na loja**: vendedor de armas e mercado negro.
- O tiro letal te deixa "ilegal" antes de acertar, se alguém vê ou ouve, para as testemunhas contarem no heat. O heat também passou a contar quem está vendo você a até 25 m.
- Quem morre com a letal é morto de novo ao carregar o save, caso o save traga a pessoa de volta.
- Correções do código original: `SpawnIntoInventory` não existe no SOD.Common atual, o GUID do Guns é `"Guns"` (confirmado) e as corrotinas foram trocadas por timers.
- Tecla de teste Shift+F7 (as duas pistolas no inventário). Nova seção **W**. O `INTEGRACAO_ARMA.md` foi removido (não é mais necessário).

### 0.4.1 — 19/10/2026
- Descoberta: o jogo vanilla **não deixa o jogador matar**, só nocautear. O Hitman vai depender do seu mod de arma (tranquilizante + tiro letal). O documento `INTEGRACAO_ARMA.md` explica o que o tiro letal precisa fazer.
- Correção: o heat conta cada morte uma vez só, mesmo que um mod de arma "mate de novo" ao carregar o save.
- Testes de morte ajustados para F10/F11 enquanto a arma não fica pronta. Nova seção **W** para quando ela estiver.

### 0.4.0 — 19/10/2026
- Policial corrupto (Fase 4): um Enforcer de verdade, escolhido quando o save começa.
  - **Primeira ligação:** liga quando o heat chega ao nível 2. Tenta o apartamento; se ninguém atender, um orelhão perto de você; e repete a cada 6 h até você atender.
  - **Pacotes:** você liga para o número dele e escolhe um: uma morte, metade ou tudo (20% de desconto). Com mandado, o preço dobra.
  - **Pagamento:** deixe o dinheiro no orelhão indicado dentro de 24 h.
  - **Substituto:** se ele morrer, outro assume 3 dias depois e liga avisando.
- Tecla de teste Shift+F5 (o policial liga agora). O F9 mostra os dados do policial.
- Novos testes: seção **P**.

### 0.3.0 — 19/10/2026
- Heat (Fase 3): cada morte causada por você soma pontos.
  - **Pontos:** contrato 10, outra morte 20; +15 por testemunha, +10 com câmera, +5 em local público.
  - **Níveis:** 1 = notícia no jornal (50); 2 = Enforcers no seu apartamento (100); 3 = procurado e caçadores (160).
  - O heat não cai sozinho e é salvo com o jogo. O policial corrupto que reduz o heat vem na próxima versão.
- Teclas de teste: Shift+F6 / Ctrl+F6 (heat ±10). O F9 mostra o heat.
- Novos testes: seção **H**.

### 0.2.0 — 19/10/2026
- Suborno (Fase 2): nova opção de diálogo com qualquer pessoa enquanto há contrato ativo.
  - **Aceita** se 2 × pagamento ≤ 40% do salário ÷ 12. Sem emprego, sempre recusa.
  - **Aceitou**: a vítima paga o dobro na hora e o contrato é encerrado sem pagamento do cliente.
  - **Recusou**: cargos de luta atacam, os outros fogem pedindo polícia.
  - Uma tentativa por contrato. A Missão 1 não tem consequência para o suborno.
- F8 e F9 agora também mostram mensagens na tela: onde o contrato foi postado, o alvo, o valor e se ele aceitaria suborno.
- Novos testes: seção **S**.

### 0.1.0 — 19/10/2026
- Fase 0: ferramentas de dev (dump F7, forçar contrato F8, listar jobs F9, matar alvo F10/F11, logs de mortes e jobs).
- Fase 1: Missão 1 "Free Lancer para delivery de pizza": anúncio, ligação, encontro com hora marcada e 15 min de tolerância, conversa com nome e local de trabalho do alvo, conclusão só por morte, entrega por baixo da porta do cliente, pagamento pelo salário do alvo.
- Dependências instaladas: SOD.Common 2.1.5, Asset Bundle Loader 2.2.1, DDSLoader 1.1.1, CommunityCaseLoader 1.1.1 (sem os casos de exemplo).
- Textos em PT-BR e inglês.
