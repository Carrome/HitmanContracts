# Mod "Hitman" para Shadows of Doubt — Escopo (rascunho 2)

Status: ESBOÇO. Nenhum código foi escrito. Este arquivo é a fonte de verdade para a implementação no VS Code.
Itens marcados **[ABERTO]** ainda precisam de decisão. Itens marcados **[VERIFICAR NO CÓDIGO]** dependem de inspecionar o jogo decompilado.

---

## 1. Visão geral

Adiciona **contratos de assassino** aos quadros de serviço (side jobs) já existentes do jogo. O jogador aceita um anúncio, é contatado por um cliente anônimo, recebe pouca informação sobre a vítima, investiga, e resolve de uma de duas formas: **matar** ou **subornar** a vítima (pedindo o dobro do que o cliente pagou). Concluído o serviço, entrega o caso e recebe, como nos side jobs normais.

Tudo que já é nativo do jogo é reaproveitado: maleta de informações, ligação em cabine em horário marcado, cliente NPC esperando em um local segurando um item, formulário de resolução e entrega do caso. O mod **não cria** itens novos, quadros novos nem tipos de missão além de assassinato.

## 2. Decisões fechadas

| Tema | Decisão |
|---|---|
| Onde ficam os anúncios | Nos quadros de serviço **existentes** (bares, lanchonetes, lobbies), misturados aos jobs normais |
| Tipos de missão | **Somente assassinato** |
| Gatilhos | (A) ligação em cabine em horário marcado; (B) encontro com cliente NPC em local e hora marcados |
| Entrega da informação | A → **maleta** deixada na cabine. B → **maleta** OU **conversa** direta |
| Tolerância de atraso | **15 min de jogo** além do horário marcado; passou disso, a missão falha |
| Conclusão | **Somente morte** da vítima, por qualquer causa (sono profundo/tranquilizante NÃO conclui) |
| Pagamento | **Entrega nativa**: formulário de resolução + entrega do caso em ponto combinado (porta do cliente ou dead drop) |
| Suborno | Vítima aceita se `2 × pagamento_do_contrato ≤ 40% × (salário ÷ 12)` |
| Suborno falhou | Vítima luta ou foge conforme o **cargo** (ver 5.3) |
| Consequência de suborno aceito (vítima viva) | **Varia por missão** (campo próprio no modelo) |
| Dependências | Usar bibliotecas da comunidade quando for o melhor caminho; embutir o código só quando for preciso alterar a dependência (como no mod da pistola tranquilizante) |
| Como o jogador mata | O jogo vanilla **não permite matar** NPCs (só nocautear). O mod TranquilizerPistol do autor foi **incorporado ao Hitman** (19/10/2026), com **duas pistolas silenciadas**. A **letal** é a Lazarus 5 silenciada do jogo, a mesma dos assassinos. A **tranquilizante** é um item novo, "Lazarus 5 tranquilizante". A munição é **infinita** e **não há recarga** (o jogo não tem sistema nem animação de recarga). As pistolas são obtidas **só no mundo ou na loja**: vendedor de armas e mercado negro. O mod é incompatível com o "Guns". |
| Valor do contrato | **Mais alto que o de um side job comum** (matar é sério) e **varia conforme o cargo da vítima** (ver 5.2) |
| Heat | Sistema de heat **incluído neste mod**: qualquer morte causada pelo jogador soma; efeitos: notícia de serial killer, enforcers investigam, mandado e perseguição; **não cai sozinho**, só abaixa pagando o **policial corrupto** (ver 11 e 11.1) |

## 3. Fluxo de uma missão

1. **Anúncio** aparece em um quadro existente (texto + telefone do anúncio).
2. **Aceite**: o jogador pega o job.
3. **Contato** (um dos dois gatilhos):
   - **A. Ligação**: o jogador liga ao telefone do anúncio; o cliente diz que a cabine X tocará às HH:MM. O jogador atende na cabine e a maleta é entregue lá.
   - **B. Encontro**: o cliente diz "me encontre em [local] às HH:MM, estarei com [item]". O NPC cliente está lá segurando o item. A informação vem por maleta ou por conversa.
4. **Atraso**: se o jogador passar de HH:MM + 15 min, a missão falha.
5. **Investigação**: o jogador recebe poucas informações (ver 6) e descobre quem é a vítima e onde está.
6. **Resolução**:
   - **Matar** a vítima, ou
   - **Subornar**: conversar com a vítima, revelar o contrato e pedir o dobro (regra em 5).
7. **Entrega**: preenche o formulário e entrega o caso no ponto combinado; recebe o pagamento.
8. **Consequências**: definidas por missão (principalmente no caso de suborno aceito).

## 4. Regras de gatilho e entrega

| Gatilho | Entrega permitida |
|---|---|
| Ligação em cabine | maleta na cabine |
| Encontro com NPC | maleta ou conversa |

- O horário marcado fica "bem próximo" do momento em que o jogador atendeu a primeira ligação (valor exato **[ABERTO]**).
- **Nota (vanilla):** a aba Detalhes do job é atualizada silenciosamente depois da maleta/ligação. O mod deve **avisar o jogador** quando isso acontecer (sugestão).

## 5. Suborno

### 5.1 Fórmula
- `oferta = 2 × pagamento_do_contrato`
- `limite = 0,40 × (salário_do_cargo ÷ 12)`
- Vítima **aceita** se `oferta ≤ limite`; senão **recusa**.
- O salário vem da tabela de cargos do jogo **[VERIFICAR NO CÓDIGO]** onde fica o valor e se ele é anual (a tabela da comunidade lista de 15.000 a ~109.900).
- Cidadão **sem emprego** (sem salário): **recusa sempre** (decidido em 19/10/2026), e então luta ou foge pela regra 5.3.

### 5.2 Calibração provisória (pagamento máximo que a vítima aceita)

| Salário da tabela | Mensal | 40% | Pagamento máx. do contrato |
|---|---|---|---|
| 15.000 | 1.250 | 500 | 250 |
| 25.000 | 2.083 | 833 | 416 |
| 49.000 | 4.083 | 1.633 | 816 |
| 100.000 | 8.333 | 3.333 | 1.666 |
| 109.900 | 9.158 | 3.663 | 1.831 |

A condição de aceitação `2P ≤ 0,40 × salário ÷ 12` equivale a `P ≤ salário ÷ 60`.

### 5.2b Pagamento do contrato (decidido: por cargo da vítima)
- O pagamento é **mais alto que o de um side job comum** e **varia conforme o cargo da vítima**: vítima mais rica ou mais perigosa paga mais.
- Fórmula exata **[ABERTO]**.
- **Atenção:** se o pagamento for apenas proporcional ao salário (`P = k × salário`), a regra do suborno vira tudo-ou-nada, porque aceitar depende só de `k ≤ 1/60` e vale igual para todas as vítimas. Para o suborno continuar uma escolha real, o pagamento precisa de outros termos (base fixa, multiplicador de risco do cargo, sorteio).
- Com pagamento alto (acima de ~1.800), ninguém da tabela aceita suborno. Conferir isso ao definir a fórmula.

### 5.3 Recusa
Se a vítima recusa:
- Cargos de risco **lutam pela vida**.
- Os demais **fogem e acionam polícia e alarme**.
- Lista de cargos que lutam (**proposta, confirmar**): Enforcer, Enforcer Captain, Security Guard (apartamento e planta industrial), Debt Collector, Weapons Dealer, Gambling Den Owner.

### 5.4 Aceitou o suborno
- A vítima segue viva; o contrato **não conclui por morte**.
- O que acontece com o cliente depende do campo `consequencia_suborno` da missão (retaliação, calote, ou nada).
- **Fechamento (decidido em 19/10/2026):** a vítima paga o dobro na hora e o contrato é encerrado **sem o pagamento do cliente** (não conta como falha). Só existe uma tentativa de suborno por contrato.
- A opção de suborno aparece com qualquer pessoa enquanto houver contrato ativo (com a informação já recebida). Quem não é o alvo só estranha. Assim a opção não entrega quem é a vítima. Isso pode ser trocado no `.cfg` (`Bribe.ShowToEveryone`).

## 6. Informação dada ao jogador

O contrato dá **poucas informações**. Cada missão sorteia um subconjunto de um pool de campos. Pool inicial (lista de ideias do dono do projeto, a refinar):

- nome e telefone
- endereço e nome
- nome do parceiro
- local de trabalho e telefone
- características de aparência
- cargo

Regras de sorteio (quantidade de campos, combinações proibidas, garantia de que a vítima seja única) **[ABERTO]**.

## 7. Modelo de dados de uma missão

Cada missão é um modelo (arquivo de dados), lido por um motor único. Campos:

| Campo | Descrição |
|---|---|
| `id` | identificador único |
| `texto_anuncio` | texto do quadro, pode ter variáveis sorteadas |
| `telefone_anuncio` | número exibido no anúncio |
| `gatilho` | `ligacao_cabine` ou `encontro` |
| `local_encontro` | tipo de local (ex.: restaurante), só para `encontro` |
| `item_na_mao` | item que o NPC cliente segura, só para `encontro` |
| `entrega` | `maleta` ou `conversa` (conversa só vale para `encontro`) |
| `regra_vitima` | como a vítima é escolhida **[ABERTO]** |
| `motivo` | texto/gerador do motivo do cliente **[ABERTO]** |
| `campos_info` | pool de campos de informação e quantos sortear |
| `pagamento` | fórmula por cargo da vítima, mais alta que side job comum (ver 5.2b) **[ABERTO]** |
| `ponto_entrega` | porta do cliente ou dead drop |
| `consequencia_suborno` | o que acontece com o cliente se a vítima aceitar e continuar viva |
| `dialogos` | ids dos diálogos (cliente, vítima, suborno, recusa) |

## 8. Exemplo (missão 1 — "Free Lancer para delivery de pizza")

- **Anúncio:** "Contrata-se Free Lancer para delivery de pizza, ligue para [telefone]".
- **Gatilho:** `encontro`. Ao ligar, o cliente diz: "me encontre no restaurante [nome] às [hora]. Estarei com [item]". A hora fica próxima da ligação; 15 min de tolerância.
- **Entrega:** conversa. O cliente diz: "Preciso que [nome da vítima] durma em um sono profundo e nunca mais me incomode. Esse vagabundo tirou fotos do pé da minha mulher na rua. Ele trabalha no [local de trabalho]".
- **Investigação:** o jogador vai ao local de trabalho, acha a ficha de funcionários (endereço, número etc.).
- **Resolução:** matar, ou conversar e subornar com o dobro (regra 5).
- **Consequência do suborno:** nenhuma nesta missão (decidido em 19/10/2026). O campo continua no modelo para as próximas missões.

## 9. Pesquisa: mods existentes

| Mod | O que oferece | Uso |
|---|---|---|
| **The Syndicate** (kropath) | Carreira de assassino com contratos (alvo completo, parcial ou frio), investigação, telefone do "Dispatcher" | **Só referência**: licença source-available (pode ler e aprender, não pode reempacotar) e conflita com outros mods de arma/assassinato |
| **Hire A Hitman** (Piepieonline) | O inverso: o jogador contrata. Mostra como criar número de telefone, tipo de caso e texto dinâmico | Referência técnica; código no GitHub dele (verificar licença) |
| **WhiteCollarSideJobs** (Piepieonline) | Adiciona side jobs novos ao quadro | Referência de como registrar jobs novos (código no GitHub dele, verificar licença) |
| **PlayerDialogAdditions** (Piepieonline) | Adiciona falas do jogador a NPCs | Base do diálogo de suborno |
| **DDSLoader / DDS Script Extensions** (Piepieonline) | Carrega textos e diálogos; scripts Lua para texto dinâmico | Textos do cliente e da vítima |
| **Asset Bundle Loader** (Piepieonline) | Cria ScriptableObjects a partir de JSON | Registrar jobs a partir de dados |
| **LeadHints** (Piepieonline) | Mostra os tipos de pista na aba Detalhes do anúncio | Referência de UI do anúncio |
| **SOD.Common** (Venomaus) | Helpers para modding | Dependência provável |

Regra: checar a licença de cada repositório antes de copiar qualquer código.

## 10. Pontos técnicos para investigar no código decompilado

1. Como um side job novo é registrado e entra no quadro (exemplo: WhiteCollarSideJobs).
2. Como o jogo faz a cabine tocar em um horário e a maleta aparecer lá.
3. Como o NPC cliente é colocado em um local segurando um item (bug conhecido: o cliente às vezes fica parado onde disse que estaria).
4. Evento de **morte** de NPC para marcar o objetivo como concluído.
5. Onde fica o **salário** do cidadão e em que unidade.
6. API do PlayerDialogAdditions para criar a opção de suborno só para quem tem o contrato ativo.
7. Como criar um formulário de resolução para o tipo "assassinato" e o ponto de entrega.
8. Como detectar e reagir à recusa (fugir, chamar polícia/alarme, atacar).
9. Salvar/carregar: jobs ativos podem quebrar ao carregar um save; os alvos e itens do job são gerados quando o job é postado.
10. **Heat:** ganchos para (a) morte causada pelo jogador, (b) testemunhas e câmeras que viram o crime, (c) jornal da cidade (manchetes novas), (d) comportamento de enforcers (investigar e perseguir), (e) estado de "procurado" que o jogo já tem. Guardar o valor do heat **dentro do save**.
11. **Policial corrupto:** como escolher e guardar um enforcer fixo no save, como um NPC liga para o jogador, e como receber o pagamento (ver 11.1).

## 11. Sistema de heat

Decidido em linhas gerais:

- **Fonte:** qualquer morte causada pelo jogador soma heat, seja de contrato ou não. Peso maior se houver testemunha ou câmera.
- **Efeitos** (cumulativos, conforme o heat sobe):
  1. **Notícia de serial killer** no jornal da cidade (o jogador é esse assassino, depois de vários assassinatos).
  2. **Enforcers investigam** o jogador.
  3. **Mandado e perseguição** no nível máximo.
- **Queda:** o heat **não cai sozinho**. Só abaixa pagando o **policial corrupto** (ver 11.1).
- **Integração:** o heat é uma pontuação própria somada **por cima** dos sistemas nativos (testemunhas, câmeras, digitais, enforcers); não substitui nenhum deles.
- Referência de estudo: o The Syndicate faz manchetes e perseguição parecidas (licença source-available; só ler e aprender).

Decidido em 19/10/2026:
- **Pesos:** morte de contrato = 10; morte fora de contrato = 20. Extras: +15 por testemunha, +10 se uma câmera viu e +5 se foi em local público.
- **Limiares:** ~~50 / 100 / 160~~. **Revisto depois do 4º teste: 30 / 60 / 90**, porque os níveis demoravam demais. Nível 1 = notícia; nível 2 = enforcers investigam e o policial corrupto liga; nível 3 = mandado e perseguição.
- **Investigação no nível 2 (decidido no 4º teste): só interrogatório, com 2 perguntas, sem revista.**
  - Um Enforcer vem até a casa do jogador e pergunta onde ele estava na hora da última morte e se conhecia a vítima.
  - Cada resposta é crível, neutra ou suspeita conforme o que a polícia poderia saber.
  - O total baixa ou sobe o heat, e depois os Enforcers vão embora.
  - Faltar, fugir ou não responder custa heat.
  - Motivo da mudança: no 4º teste, a ronda vanilla do `CallEnforcers` só revistava a casa com falas de rádio, sem falar com o jogador.
  - Detalhes de implementação na seção 12.
- **No máximo (nível 3):** mandado mais **caçadores**. De tempos em tempos um NPC armado (assassino de aluguel) vem atrás do jogador. Isso dura enquanto o heat estiver no nível 3.
- **Anúncios:** o heat **não** afeta os anúncios de contrato.

### 11.1 Policial corrupto (única forma de reduzir o heat)

Decidido:
- **Identidade:** um **enforcer existente**, escolhido pelo mod quando o save começa e **guardado no save**. É sempre o mesmo e a ficha dele (nome, local de trabalho) existe no jogo, então pode ser investigado.
- **Contato:** **ele liga primeiro** quando o heat chega ao **nível 2** (enforcers investigam), oferecendo "ajuda".
- **Pagamento:** **preço por ponto**. O jogador escolhe quanto heat remover e paga proporcionalmente. Preço de referência: remover o heat de **uma morte custa 50% do pagamento médio de um contrato**.
- **Entrega do dinheiro:** **dead drop combinado**. O policial diz por telefone onde deixar o dinheiro e o jogador deixa lá (reaproveita a entrega nativa dos side jobs).
- **Riscos:** **nenhum**. O policial sempre cumpre; o desafio é só juntar dinheiro.
- **Se o jogador matar o policial:** **outro enforcer assume** depois de alguns dias de jogo. O mod escolhe o novo, guarda no save, e o novo policial liga ao jogador avisando que assumiu. Vale também quando o enforcer escolhido não existe ao carregar o save.

Decidido em 19/10/2026:
- **Telefone:** ele liga primeiro para o **apartamento do jogador**. Se ninguém atender, toca uma **cabine perto do jogador**.
- **Insistência:** enquanto o jogador não atender, ele tenta de novo **a cada 6 horas** de jogo.
- **Pagamento em pacotes fixos:** remover uma morte (10 pontos), metade do heat, ou o heat inteiro (com desconto). O preço do ponto vem da referência acima: uma morte custa 50% do pagamento médio de um contrato.
- **Com mandado ativo (nível 3):** ele continua ajudando, mas **cobra o dobro** por ponto.
- **Substituto:** outro enforcer assume **3 dias de jogo** depois que o policial morre. É um valor provisório e configurável.

## 12. Notas de implementação (rodada 1, 19/10/2026)

O que foi descoberto lendo o jogo e as bibliotecas, e o que foi decidido para a v0.1.0. Testes em jogo pendentes: ver `CHECKLIST_TESTES.md`.

**Dependências finais:** **SOD.Common 2.0.7**, Asset Bundle Loader 2.2.1, DDSLoader 1.1.1 e **CommunityCaseLoader 1.1.1**.
- **Por que 2.0.7:** o jogo do autor é o build de 26/09/2024. O SOD.Common 2.1.3+ faz patch no menu de mods de builds mais novos (`ModSettingsData.ModSource`), falha ao carregar e derruba o Hitman, como aconteceu no 1º teste. As versões 2.1.0–2.1.2 citam `Human.InteractionDialogInstance`, que também não existe nesse build.
- **Como checar compatibilidade:** `tools/refcheck` compara as referências de uma DLL com o `Assembly-CSharp` do jogo. Rode antes de atualizar qualquer dependência.
- **Patches Harmony e funções fundidas pelo IL2CPP:** o compilador funde métodos com o mesmo código de máquina (vazios, `return false`…). Um patch nesses métodos roda para **todos** os que compartilham o código, com o `this` errado, e derruba o jogo. Foi o 2º teste: `SideJob.OnGooseChaseCallTriggered` estava fundido com 4.245 métodos e `RevengeObjective.ManualTrigger` com 386. `tools/foldcheck` lê o `GameAssembly.dll` (via LibCpp2IL do BepInEx) e marca os métodos fundidos. O `pack.ps1` roda a checagem em todos os patches e aborta se achar algum.
- Por isso o objetivo `HitmanKill` (`answerMethod: ManualTrigger`) **não** recebe patch: o mod marca a pergunta como concluída direto (`KillTracker.CompleteKill`). O CommunityCaseLoader é quem lê os `murdermanifest.sodso.json`. Todas são MIT.
- **Ler o código nativo:** `tools/disasm <jogo> Tipo::Método | fields:Tipo | xref:Tipo::Método` desmonta um método com as chamadas e as strings resolvidas, lista os offsets dos campos e mostra quem chama um método. Foi assim que o 3º teste foi resolvido (abaixo).

**Morte causada pelo jogador (3º teste, v0.5.4):**
- **O problema:** neste build, o construtor de `Human.Death` lê o `MurderController.Murder` sem checar null. `Human.Murder(killer, setTimeOfDeath: true, murder: null, …)` lança no meio. A vítima fica com `isDead`, mas sem registro de morte, corpo, lista de mortos ou `DeadPeopleJobCheck`, e nenhum postfix de `Human.Murder` roda: foi por isso que o heat não subia.
- **O dano do jogo tem o mesmo problema:** `Citizen.RecieveDamage` com `enableKill` chama `Murder(…, true, <caso do assassino ou null>, …)`, que também quebra quando o assassino é o jogador.
- **A correção (`WeaponKills.MurderByPlayer`):** um `Murder` alocado com `il2cpp_object_new`, sem rodar o construtor (que iniciaria um caso inteiro), com `murderID = -1`. A arma não vai na chamada, porque com um `Murder` presente o jogo chamaria `GenerateMoniker`; ela é gravada depois em `death.weapon`. Os três chamadores de `Death.GetMurder()` (`CitizenBehaviour.GameWorldCheck`, `SaveStateController.LoadSaveState` e `Strings.GetScopeObject`) aceitam null. A pistola letal deixou de usar `enableKill`.
- **No save:** o `death` vai em `CitizenStateSave` e o `LoadSaveState` refaz `Human.Murder(killer, false, null, null)` para cada corpo salvo. Entre `OnBeforeLoad` e o fim do `OnAfterLoad` (`ContractStore.Loading`), os postfixes de heat e de contrato ignoram essas chamadas.
- **`DeadPeopleJobCheck`:** só encerra jobs `posted` **não aceitos** cujo cliente ou alvo está morto ou é o assassino de um caso ativo ("features a dead person or killer"). A proteção do Hitman agora só vale para contrato aceito.
- **Postagem (F8):** `JobCreationCheck` começa sobrescrevendo `desiredActiveInstances` de cada preset com `JobPreset.GetFrequencyForSocialCreditLevel()` e tira do quadro os jobs não aceitos que passam desse número. O F8 soma uma vaga por um postfix nesse método, e ela dura a sessão.

**Como um job novo entra no jogo (item 10.1):** só com dados. O `JobPreset` em JSON faz `copyFrom` de um preset vanilla, e o jogo instancia o job pelo campo `subClass` (vazio nos jobs de vingança). Os textos ficam em DDS (`.tree`/`.msg`/`.block` + `dds.blocks.csv`) e os nomes de caso e objetivo em `missions.postings.csv`, um por idioma.

**Base escolhida:** `VandalismHome_D3`. Ele já tem os intros `Dossier-MeetUpSpecificLocation` (encontro), `Dossier-TelephoneGooseChase` (cabine) e `Dossier-PhotoByStaff`, o hand-in `SlipUnderDoor` e o sorteio de pistas (`leadPoolData` + `BasicLeadPool`, que equivale à seção 6).

**Gatilhos (itens 10.2 e 10.3):**
- O **encontro vanilla não tem horário**: o cliente fica sentado no local com um item (`|sidejob.meetitem|`) até o jogador chegar. O horário e a tolerância de 15 min (seção 4) são do mod: `MeetDelayMinutes` (padrão 60, provisório para o "bem próximo" da seção 4) e `LateToleranceMinutes` (15).
- A **cabine** já tem hora vanilla (`|sidejob.calltime.time24|`).
- A conversa no encontro (`IntroMeetUpSpecific2`) já é a "entrega por conversa".

**Conclusão por morte (item 10.4):** a pergunta de resolução é do tipo `revengeObjective`, com o objetivo novo `HitmanKill` (`answerMethod: ManualTrigger`). O mod marca a pergunta como cumprida quando o alvo morre, via postfix em `Human.Murder` mais uma varredura por minuto. O cancelamento automático do jogo quando o alvo morre é bloqueado; se o cliente morrer, o contrato acaba.

**Salário (item 10.5):** é `Occupation.salary` (float). A unidade ainda vai ser confirmada pelo dump.

**Pagamento provisório (5.2b):** `P = (400 + salário/120) × risco × sorteio(0,8–1,2)`, arredondado a 10 e limitado a 300–5000. O risco é 1,5 para os cargos que lutam. O sorteio usa o `jobID` como semente. Tudo fica no `.cfg`.

**Cargos que lutam (5.3), com os nomes do jogo:** `Enforcer`, `EnforcerCaptain`, `SecurityGuard`, `SecurityGuardDesk`, `DebtCollector`, `WeaponsDealer`, `GamblingDenOwner` e mais `HiredKiller` (proposta). Confirmar.

**Missão 1 (seção 8), o que mudou do rascunho:**
- O motivo ficou neutro em gênero ("Essa praga vive tirando fotos dos pés da minha família na rua"), porque o alvo sorteado pode ser de qualquer gênero e o cliente pode não ter esposa. O texto é editável no CSV.
- Por enquanto o alvo precisa ter emprego e não pode ser Enforcer (`HitmanPizzaMotive`).
- A dificuldade é D0, para o contrato aparecer desde o começo do jogo.
- A entrega é por baixo da porta do cliente; o dead drop fica para depois.

**Heat (v0.3.0), como foi implementado:**
- **Fonte:** postfix em `Human.Murder` quando o assassino é o jogador.
- **Testemunhas:** `Player.witnessesToIllegalActivity`, no máximo 4.
- **Câmera:** o local tem `securityCameras`.
- **Público:** `isOutside` ou `IsPublicallyOpen`.
- **Efeitos:**
  - **Jornal:** troca o `mainArticle` da próxima edição (`GenerateNewNewspaper`) por um artigo DDS próprio, um por nível.
  - **Nível 2:** interrogatório (v0.5.5, abaixo) a cada 24 h. **Nível 3:** `CallEnforcers` no apartamento a cada 8 h, com a resposta do próprio jogo ao procurado.
  - **Nível 3:** `SetWantedInBuilding` no prédio atual a cada minuto. Os caçadores são cidadãos `HiredKiller`, ou na falta deles outros cargos de luta que não sejam Enforcer, usando `ai.SetPersue` no jogador a cada 12 h.
- Cada efeito pode ser desligado no `.cfg`. O heat fica no mesmo JSON do save que os contratos.

**Policial corrupto (v0.4.0), como foi implementado:**
- **Escolha:** um `Enforcer` vivo, de preferência com telefone em casa, sorteado no primeiro minuto do save e guardado no JSON.
- **Número:** um número falso (faixa 6xx-xxxx) registrado com `TelephoneController.AddFakeNumber`. Ele vai para o caderno do jogador no primeiro contato.
- **Ligação dele (corrigida na v0.5.5):** `CreateNewCall` com `CallSource(CallType.dds, árvore HitmanCop_Call)`.
  - Ao atender, o jogo faz quem ligou falar a árvore para quem atendeu (`PhoneCall.SetCallState` → `caller.ExecuteConversationTree`), como na ligação do LEM no capítulo 1.
  - O texto do bloco único é trocado antes de cada ligação.
  - A versão antiga usava `CallType.player` com um diálogo de saudação, que é o sentido inverso (o jogador fala para quem atende).
  - Se ninguém atender o apartamento, toca um orelhão a até 40 m do jogador. Atender ou perder é detectado por postfix em `PhoneCall.SetCallState`.
- **Ligação do jogador:** o número falso tem `CallType.fakeOutbound` com o diálogo de saudação (a resposta dele), mais três opções de telefone (`telephoneCallOption`) com pontos e preço. Ao escolher, ele sorteia um orelhão entre 60 m e 400 m de distância como dead drop, com prazo de 24 h.
- **Diálogos no telefone com o SOD.Common 2.0.7:** o prefix dele em `DialogController.ExecuteDialog` faz `saysTo.isActor.Cast<Citizen>()`. No telefone, `isActor` é o `Player` (ligação para o jogador) ou null (o jogador ligando para um número), então a conversão lança erro e o jogo descarta o diálogo. `PhoneDialogs.cs` roda antes (`Priority.First`) e, quando o ouvinte não é um `Citizen`, decide o `forceSuccess` pela lógica do próprio diálogo. Com `forceSuccess` decidido, o prefix do SOD.Common não faz nada.
- **Pagamento:** ao chegar a menos de 3 m do orelhão com o dinheiro, o envelope é deixado automaticamente. Ainda não há uma interação própria de "deixar envelope".
- **Morte do policial:** cancela o envelope pendente, e o substituto liga 3 dias depois.

**Interrogatório (v0.5.5), como foi implementado** (`Heat/Interrogation.cs`):
- **Visita:** no nível 2, a investigação fica pendente até o jogador estar em casa (`isHome` ou `currentGameLocation == home`). Aí o mod faz `CallEnforcers(casa)`.
- **Interrogador:** o primeiro Enforcer da chamada (`enforcerCalls[casa].response`) que fica a até 12 m do jogador, no mesmo local ou a 4 m. Na falta dele, qualquer um de `GameplayController.enforcers`. O policial corrupto nunca é escolhido.
- **Abertura:** ele fala pelo `speechController.Speak("dds.blocks", bloco)` e a fala também aparece na tela.
- **Perguntas:** as respostas são 6 diálogos do SOD.Common (3 por pergunta), visíveis só com esse Enforcer e na etapa certa.
  - A resposta à pergunta 1 tem `endsDialog = false`, e a fala dele já traz a pergunta 2.
  - A reação e a conclusão são montadas em `ShouldDialogSucceedOverride`.
- **Pontos:** crível −1, neutro 0, suspeito +1.
  - "Em casa" vale +1 se a última morte teve testemunha ou câmera (`LastKillSeen`).
  - "Nunca ouvi falar" vale +1 se a vítima morava no prédio do jogador (`LastVictimNeighbour`).
  - "Não é da sua conta" e "Quero um advogado" valem sempre +1.
  - Total ≤ −1: `HeatWhenConvinced` (−10). Total 0: nada. Total ≥ 1: `HeatPerSuspiciousPoint` × total.
- **Faltas (`HeatWhenAvoided`, +10):** o jogador fica fora de casa por `AvoidHours` (24 h), sai no meio, ou não responde em `WaitMinutes` (45).
- **Fim:** `enforcerCalls[casa].state = completed`, para os Enforcers irem embora. Ainda falta confirmar em jogo que isso basta.
- **Save:** a visita em andamento não é salva (`[JsonIgnore]`); só a pendência (`VisitDue`). Depois de carregar, a visita acontece de novo.

**Menu de teste (v0.5.7, `Dev/DevMenu.cs` + `Dev/DevActions.cs`):**
- **Tecla:** Insert (`[Dev] KeyMenu`).
- **Interface:** UniverseLib 1.5.1, o mesmo arquivo que o SOD.Common e o Asset Bundle Loader instalam. O SOD.Common já chama `Universe.Init`; o Hitman chama de novo, o que é seguro e só agenda o callback, com `Force_Unlock_Mouse`. O painel é um `PanelBase` montado na primeira abertura.
- **Enquanto aberto:** `Player.EnablePlayerMouseLook(false)` e `EnablePlayerMovement(false)`, e o `WeaponController` ignora os botões.
- **Opções:**
  - as das teclas F;
  - teletransporte com `Player.Teleport(node)`, usando `post.node`, `poster/purp.currentNode`, `chosenMeetingPoint.node`, o nó do orelhão e `FindSafeTeleport(casa)`;
  - as chaves de debug de `Game.Instance`: `invinciblePlayer`, `invisiblePlayer`, `inaudiblePlayer`, `disableFallDamage`, `disableTrespass`, `pauseAI`, `noReactOnAttack`, `keysToTheCity`;
  - `EnableGhostMovement`;
  - `SetTimeSpeed` e `SetGameTime(+h)`;
  - limpar procurado: `CityBuildings.buildingDirectory[].wantedInBuilding = 0`, mais todos os NPCs esquecendo o jogador;
  - marcar o contrato cumprido sem matar (`KillTracker.CompleteKill`);
  - listar as perguntas do caso.

**Contrato depois do aceite (4º teste):** `SideJob.AcceptJob` copia `job.resolveQuestions` para `thisCase.resolveQuestions` e esvazia a lista do job (lido no código nativo). Toda leitura das perguntas passa por `ContractRegistry.Questions(job)`, que usa a lista do caso quando ela existe. Antes disso, a morte do alvo não marcava a pergunta "matar" e a entrega não validava.

**Pistolas (v0.5.0), como foi implementado** (código em `src/SOD.Hitman/Weapons`, vindo do TranquilizerPistol):
- **Mirar e atirar:** mirar no botão secundário e atirar no primário (fluxo do Guns, MIT). O disparo usa `Toolbox.Shoot`. Um acerto até 1 s depois do tiro conta como desse tiro, caso a bala não seja instantânea.
- **Tranquilizante (refeita na v0.5.6, `Weapons/Tranquilizer.cs`):** decidido 19/10/2026: o NPC fica **3 minutos reais** apagado, a perseguição ou o combate contra o jogador para na hora, e ele esquece quem o atacou.
  - **Lido no código nativo:** `SetKO(forced)` faz `koTime = gameTime + forcedDuration`, em **horas de jogo**. A versão antiga passava 120, o que dava uns 5 dias de sono.
  - **Lido no código nativo:** `Citizen.RecieveDamage` vindo do jogador faz o NPC perseguir quem atirou (`SetPersue`) mesmo com dano zero. O jogador fica no `persuedBy`, que mantém o ícone de olho.
  - **Agora:** o prefix pula o `RecieveDamage` original para o dardo. O mod chama `SetKO` e conta o sono num relógio próprio, que soma `Time.deltaTime` só com `SessionData.play` ligado. Ao fim, põe `koTime = agora` para o jogo levantar o NPC do jeito dele; 5 s depois, se ele ainda estiver caído, chama `SetKO(false)`.
  - **Reserva:** o `forcedDuration` passado ao jogo é o mesmo tempo convertido em horas de jogo pela taxa medida (`gameTimePassedThisFrame / deltaTime`), com folga de 25%. Ela vale quando um load apaga o relógio do mod.
  - **"Esquecer":** `CancelCombat`, `CancelPersue`, `SetPersuit(false)`, `SetSeesOnPersuit(false)`, `ResetInvestigate`, `SetReactionState(none)`, a retirada do jogador de `trackedTargets`, `ClearSeesIllegal`, `player.RemovePersuedBy(npc)`, a retirada do NPC de `witnessesToIllegalActivity` e `UpdateIllegalStatus`. Roda no dardo, ao acordar e 5 s depois.
- **Letal:** dano cheio, com o `enableKill` do jogo desligado (ver "Morte causada pelo jogador" acima). Um acerto que nocauteia, zera a vida ou chega a `ShotsToKill` (padrão 1) mata com `SetHealth(0)` + `Human.Murder(Player, true, <Murder em branco>, …)`, mais a lista de mortos e uma nova escolha do assassino ou da vítima da IA (padrão do LethalActionReborn, MIT). O save do jogo guarda o registro de morte. Ao carregar, o mod só refaz as mortes que voltaram sem registro.
- **Ilegalidade:** o tiro letal marca o jogador como ilegal **antes** de acertar, quando alguém vê ou ouve, para as testemunhas contarem no heat. O dardo é legal por padrão.
- **Item novo:** a tranquilizante é `HitmanTranquilizerPistol`, um `copyFrom` da `SemiAutomaticPistolSilenced` carregado pelo CCL. O nome fica em `evidence.names.csv`. As duas pistolas entram em `itemsSold` dos `MenuPreset` `WeaponsDealer` e `BlackmarketTrader`.
- **Por que a letal é a pistola vanilla:** se a vanilla virasse a tranquilizante, toda arma de crime silenciada viraria "pistola de dardos" nos casos de assassinato. É possível inverter pelo `.cfg`.
- **Avisos de licença:** Guns e LethalActionReborn estão em `THIRD_PARTY_NOTICES.md`, que vai no pacote.

**Para a Fase 2 (suborno):** as respostas de diálogo do jogo (`AISpeechPreset`) já têm `startCombat` e `flee`, que servem para a recusa (5.3).

## 13. Pendências (para as próximas conversas)

1. **Fórmula de pagamento** por cargo, garantindo que o suborno continue uma escolha (ver 5.2b).
2. **Heat:** pesos, limiares e efeito no máximo (ver 11), e os detalhes do **policial corrupto** (ver 11.1).
3. **Escolha da vítima** e **motivo** do cliente (inclusive se o motivo pode ser mentira).
4. Pool final de **informações** e regras de sorteio.
5. ~~Tratamento de vítima **sem emprego**.~~ Decidido: recusa sempre (5.1).
6. Lista final de cargos que **lutam** (proposta com os nomes do jogo na seção 12).
7. ~~Como **fechar** um contrato subornado e se paga.~~ Decidido: só o suborno, contrato encerrado (5.4).
8. **Catálogo de missões** e as aleatoriedades de cada uma.
